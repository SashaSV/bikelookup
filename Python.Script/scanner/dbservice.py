from pymongo import MongoClient
from dataclasses import dataclass
import json
from bson.objectid import ObjectId
import pytils.translit
import os
from urllib.request import urlretrieve
from scanner.modeldb import Adresses, Venodr, Category, Picture, Manufacturer, UrlRecord, TierPrice, Product, SpecificationAttribute

from datetime import datetime

@dataclass
class DataScraps:
    url: str = ''
    name: str = ''
    sku: str = ''
    manufacturer: str = ''
    category: str = ''
    price: float = 0.0
    oldprice: float = 0.0
    vendor: str = ''
    available: str = ''
    techs: list[str] = None
    images: list[str] = None
    model: str = ''
    color: str = ''
    year: int = 0
    memory: str = ''
    display: str = ''
    CPU: str = ''
    
    def __post_init__(self):
        self.techs = []
        self.images = []

class DataScrapsEncoder(json.JSONEncoder):
    def default(self, obj):
            return obj.__dict__

class obj(object):
    def __init__(self, dict_):
        self.__dict__.update(dict_)

def dict2obj(d):
    return json.loads(json.dumps(d), object_hook=obj)


def find_document(collection, elements, multiple=False, retfields = None):
    if multiple:
        results = collection.find(elements, retfields)
        return [r for r in results]
    else:
        return collection.find_one(elements)
    
def update_document(collection, query_elements, new_values):
    """ Function to update a single document in a collection.
    """
    collection.update_one(query_elements, {'$set': new_values})

def insert_document(collection, data):
    """ Function to insert a document into a collection and
    return the document's id.
    """

    return collection.insert_one(data).inserted_id

def delete_document(collection, query):
    """ Function to delete a single document from a collection.
    """
    collection.delete_one(query)


def check_product(db, data:list[DataScraps]):

    collaction = db.Product

    simpleTemplateId = find_document(db.ProductTemplate, {'Name': 'Simple product'})
    groupTemplateId = find_document(db.ProductTemplate, {'Name': 'Grouped product (with variants)'})
    groupTemplateId = groupTemplateId['_id'] if not groupTemplateId is None else None
    deliveryDateId = find_document(db.DeliveryDate, {})
    taxCategoryId = find_document(db.TaxCategory, {})
    warehouseId = find_document(db.Warehouse, {})
    unitId = find_document(db.MeasureUnit, {})

    constData = Product(ProductTemplateId = simpleTemplateId['_id'] if not simpleTemplateId is None else None,
                        DeliveryDateId = deliveryDateId['_id'] if not deliveryDateId is None else None,
                        TaxCategoryId = taxCategoryId['_id'] if not taxCategoryId is None else None,
                        WarehouseId = warehouseId['_id'] if not warehouseId is None else None,
                        UnitId = unitId['_id'] if not unitId is None else None,
                        ) 
    
    cnt = 0
    for d in data:
        vendorid = check_vendor(db, d.vendor)
        vendorid = vendorid.get('_id')  if not vendorid is None else None
        p = find_document(collaction, {'Sku': d.sku, 'VendorId': vendorid})
        constData.VendorId = vendorid
        ##
        manufacturer = d.manufacturer
        
        name = d.name.strip()
        cnt = cnt + 1
        print('{0}. Загрузка {1}'.format(cnt, name))

        #d.techs.available = d.available
        #d.techs['category'] = d.category

        constData.ProductTemplateId = groupTemplateId
        constData.VisibleIndividually = True
        constData.ProductTypeId = 10

        p_main = check_mainproduct(db, d, constData)

        if p == None:
            constData.ProductTemplateId = groupTemplateId
            constData.VisibleIndividually = False
            constData.ProductTypeId = 5
            new_p = create_product(db, d, constData)
            new_p.ParentGroupedProductId = p_main.get('_id')
            new_p.Name = '{0} - {1}'.format(name, d.vendor)

            insert_document(collaction, new_p.__dict__)
            p = new_p.__dict__

        check_rel_сategories_to_product(db, p, d.category)
        check_rel_manufacturers_to_product(db, p, manufacturer)

        sao = check_specificationattributeoption_by_name(db, 'Available', d.available)
        sa = check_specificationattribute_by_name(db, 'Available')
        check_productspecificationattributeoption(db, p, sa, sao)

        print('     Подгрузка характеристик')
        for prop_name, prop_value in d.techs.__dict__.items():
            if prop_value.strip() == '':
                continue

            sa = check_specificationattribute_by_name(db, prop_name)

            sao = check_specificationattributeoption_by_name(db, prop_name, prop_value)
            sa = check_specificationattribute_by_name(db, prop_name)
            check_productspecificationattributeoption(db, p_main, sa, sao)

        images = [] if p_main.get('ProductPictures') is None else p_main.get('ProductPictures')

        if len(images) == 0:
            print('     Подгрузка картинок')
            for urlimage in d.images:
                check_image_to_product(db, p_main, urlimage)
    return p

def check_vendor(db, vendorName):
    collaction = db.Vendor
    v = find_document(collaction, {'Name': vendorName})
    if v is None:
        adresses = Adresses() 
        new_v = Venodr(Name=vendorName)
        
        new_v.SeName = get_sename(vendorName, db, new_v._id, 'Vendor', '')
        new_v.Email = "sales@"+vendorName
        new_v.Address = adresses.__dict__

        print({'CreatedOnUtc':datetime.now()})
        
        insert_document(collaction, new_v.__dict__)
        v = new_v.__dict__
    return v

def add_specificationattributeoption(db, sa, sao, prop_name, color_hex = None, parentSPO = None):
    id_ = str(ObjectId())

    new_sao = {
        '_id': id_,
        'Name': prop_name,
        'SeName': get_sename(prop_name, db, id_, 'SpecificationAttributeOption', ''),
        'ColorSquaresRgb': '',
        'DisplayOrder': 0,
        'ParentSpecificationAttrOptionId': '' if parentSPO == None else parentSPO,
        'Locales': []
    }

    if not color_hex is None:
        new_sao['ColorSquaresRgb'] = color_hex

    sao.append(new_sao)

    update_document(db.SpecificationAttribute, {'_id': sa.get('_id')},
                    {'SpecificationAttributeOptions': sao})
    return new_sao

def check_specificationattributeoption_by_name(db, prop_name, prop_value, color_hex = None, parentSPO = None):
    sa = check_specificationattribute_by_name(db, prop_name)
    sao = sa.get('SpecificationAttributeOptions')
    sao = sao if not sao is None else [] 
    sao_ret = None

    for ind, a in enumerate(sao):
        if a['Name'].lower().strip() == prop_value.lower().strip():
             sao_ret = sao[ind]

    if sao_ret is None:
        sao_ret = add_specificationattributeoption(db, sa, sao, prop_value, color_hex = color_hex, parentSPO = parentSPO)

    return sao_ret

def add_productspecificationattributeoption(db, p, sa, sao, psao):
    new_psao = {
        '_id': str(ObjectId()),
        'AttributeTypeId': 0,
        'SpecificationAttributeId': sa.get('_id'),
        'SpecificationAttributeOptionId': sao['_id'],
        'CustomValue': 'null',
        'AllowFiltering': False,
        'ShowOnProductPage': True,
        'ShowOnSellerPage': True,
        'DisplayOrder': 0,
        'Locales': []
    }

    psao.append(new_psao)
    update_document(db.Product, {'_id': p['_id']},
                    {'ProductSpecificationAttributes': psao})
    return psao

def check_productspecificationattributeoption(db, p, sa, sao):
    psao_ret = None
    psao = p.get('ProductSpecificationAttributes')

    psao = psao if not psao is None else []

    for ind, a in enumerate(psao):
        if a['SpecificationAttributeId'] == sa.get('_id') and a['SpecificationAttributeOptionId'] == sao['_id']:
            psao_ret = psao[ind]
            spOption = sa['Name']

            do = 99 #if spOption.get('displayOrderOnTabProduct') is None else spOption.get('displayOrderOnTabProduct')

            psao[ind]['AllowFiltering'] = True
            psao[ind]['ShowOnProductPage'] = True
            psao[ind]['ShowOnSellerPage'] = True
            psao[ind]['DisplayOrder'] = do

            update_document(db.Product, {'_id': p['_id']},
                    {'ProductSpecificationAttributes': psao})

            break

    if psao_ret is None:
        add_productspecificationattributeoption(db, p, sa, sao, psao)

    return

def check_specificationattribute_by_name(db, prop_name):
    collaction = db.SpecificationAttribute

    sa = find_document(collaction, {'Name': prop_name})
    if sa == None:
        sa = SpecificationAttribute(Name = prop_name,
                                    Locales = get_locals(db, prop_name))
        
        sa.SeName = get_sename(prop_name, db, sa._id, 'SpecificationAttribute', '')
        sa = sa.__dict__
        insert_document(collaction, sa)
    else:
        spOption = prop_name
        #do = 99 if spOption.get('displayOrderOnTabFilter') is None else spOption.get('displayOrderOnTabFilter')
        updExpr ={}
        updExpr['DisplayOrder'] = 99
        update_document(collaction, {'_id': sa.get('_id')}, updExpr)
    return sa

def get_locals(db, prop_name):
    collaction = db.Language
    lngs = find_document(collaction, {}, multiple = True)
    locals = []
    for l in lngs:
        name_local = prop_name
        if name_local is None:
            continue
        #name_local = name_local.get(l['Name'].lower())
        if name_local is None:
            continue
        id_ = str(ObjectId())
        new_lng = {
            '_id': id_,
            'LanguageId': l['_id'],
            'LocaleKey': 'Name',
            'LocaleValue': name_local
        }
        locals.append(new_lng)
    return locals

def check_mainproduct(db, d, constData):
    collaction = db.Product
    p_main = find_document(collaction, {'Sku': d.sku, 'VendorId': ''})
    if p_main == None:
        new_p = create_product(db, d, constData)
        new_p.Url = ''
        new_p.ShortDescription = ''
        new_p.VendorId = ''
        new_p.ProductCategories = []
        insert_document(collaction, new_p.__dict__)
        p_main = new_p.__dict__
    else:
        updateExpr = {}
        updateExpr['Name'] = d.name
        update_document(collaction, {'_id': p_main.get('_id')}, updateExpr)

    check_rel_сategories_to_product(db, p_main, d.category)
    check_rel_manufacturers_to_product(db, p_main, d.manufacturer)
    return p_main

def check_сategories(db, c_name) -> Category:
    collaction = db.Category
    c_main = find_document(collaction, {'Name': c_name})
    
    if c_main == None:
        categoryTemplateId = find_document(db.CategoryTemplate, {})
        categoryTemplateId = categoryTemplateId['_id'] if not categoryTemplateId is None else None
        c_main = Category(Name=c_name, CategoryTemplateId=categoryTemplateId)
        c_main.SeName = get_sename(c_name, db, c_main._id, 'Category', '')
        c_main = c_main.__dict__
        insert_document(collaction, c_main)

    return c_main

def check_rel_сategories_to_product(db, p, c_name):
    cat_ret = None
    cat = p.get('ProductCategories')
    cat = cat if not cat is None else []

    c = check_сategories(db, c_name)
    for ind, cr in enumerate(cat):
        if cr['CategoryId'] == c.get('_id'):
            cat_ret = cat[ind]

    if cat_ret is None:
        cat_ret = add_rel_сategories_to_product(db, p, c, cat)

def get_file_picture_name(pictureId:str) -> str:
        CURR_DIR = os.getcwd()
        CURR_DIR = CURR_DIR.replace('\\Python.Script', '')
        file_catalog = '{0}\\Grand.Web\\wwwroot\\content\\images\\'.format(CURR_DIR)
        
        try:
            os.stat(file_catalog)
        except:
            os.mkdir(file_catalog)

        return '{0}{1}_0.jpeg'.format(file_catalog, pictureId)

def check_picture(db, pictureId, urlimage, productname):
    collaction = db.Picture
    p_main = find_document(collaction, {'UrlImage': urlimage})
    if p_main == None:
        id_ = str(ObjectId())
        filename = get_file_picture_name(id_)
        load_image(urlimage, filename)

        in_file = open(filename, "rb")  # opening for [r]eading as [b]inary
        data = in_file.read()  # if you only wanted to read 512 bytes, do .read(512)
        in_file.close()

        p = find_document(collaction, {'_id': pictureId})
        
        if not p is None:
            if p['PictureBinary'] == data:
                p_main = p
                if os.path.exists(filename):
                    os.remove(filename)

        if p_main == None:
            p_main = Picture(PictureBinary=data)
            p_main._id = id_ 
            p_main.SeoFilename = get_sename(productname, db)
            p_main.UrlImage = urlimage
            p_main.AltAttribute = productname
            p_main.TitleAttribute = productname

            p_main = p_main.__dict__
            insert_document(collaction, p_main)

    return p_main

def check_image_to_product(db, p, urlimage):
    pict_ret = None
    pictures = p['ProductPictures']
    pName = p['Name']
    for ind, pictFP in enumerate(pictures):
        pictureId = pictFP['PictureId']
        picture = check_picture(db, pictureId, urlimage, pName)
        if pictureId == picture['_id']:
            pict_ret = pictures[ind]

    if pict_ret is None:
        picture = check_picture(db, '', urlimage, pName)
        add_rel_picture_to_product(db, p, picture, pictures, pName)

def add_rel_picture_to_product(db, p, picture, pictures, productname):

    new_rel = {
        '_id': str(ObjectId()),
        'PictureId': picture['_id'],
        'DisplayOrder': 1,
        'MimeType': None,
        'SeoFilename': get_sename(productname, db),
        'AltAttribute': productname,
        'TitleAttribute': productname
    }

    pictures.append(new_rel)
    update_document(db.Product, {'_id': p['_id']},
                    {'ProductPictures': pictures})
    return pictures

def add_rel_сategories_to_product(db, p, c, cat):

    new_rel = {
        '_id': str(ObjectId()),
        'CategoryId': c.get('_id'),
        'IsFeaturedProduct': False,
        'DisplayOrder': 0
    }

    cat.append(new_rel)
    update_document(db.Product, {'_id': p.get('_id')},
                    {'ProductCategories': cat})
    return cat

def check_manufacturers(db, manufacturer):
    collaction = db.Manufacturer

    if type(manufacturer) is str:
        brandname = manufacturer
        brandimg = None
    else:
        brandname = manufacturer.get('brand')
        brandimg = manufacturer.get('brandimg')

    if brandname is None:
        brandname = manufacturer

    m_main = find_document(collaction, {'Name': brandname})

    pictureId = None
    if not brandimg is None:
        pId = ''
        if not m_main is None:
            pId = m_main.get('PictureId') if not m_main.get('PictureId') is None else ''

        picture = check_picture(db, pId, brandimg, brandname)
        pictureId = picture['_id']

    if m_main == None:
        manufacturerTemplateId = find_document(db.ManufacturerTemplate, {})
        manufacturerTemplateId = manufacturerTemplateId['_id'] if not manufacturerTemplateId is None else None

        m_main = Manufacturer(Name=brandname, 
                              ManufacturerTemplateId = manufacturerTemplateId,
                              PictureId = pictureId)
        m_main.SeName = get_sename(brandname, db, m_main._id, 'Manufacturer', '')
        m_main = m_main.__dict__
        insert_document(collaction, m_main)
    else:
        if not pictureId is None:
            updExpr = {}
            updExpr['PictureId'] = pictureId
            update_document(collaction, {'_id': m_main._id}, updExpr)
    return m_main

def check_rel_manufacturers_to_product(db, p, m_name):
    mun_ret = None
    man = p['ProductManufacturers']
    man = man if not man is None else []
    m = check_manufacturers(db, m_name)

    for ind, mr in enumerate(man):
        if mr['ManufacturerId'] == m['_id']:
            mun_ret = man[ind]

    if mun_ret is None:
        mun_ret = add_rel_manufacturers_to_product(db, p, m, man)

def add_rel_manufacturers_to_product(db, p, c, cat):

    new_rel = {
        '_id': str(ObjectId()),
        'ManufacturerId': c['_id'],
        'IsFeaturedProduct': False,
        'DisplayOrder': 0
    }

    cat.append(new_rel)
    update_document(db.Product, {'_id': p['_id']},
                    {'ProductManufacturers': cat})
    return cat

def create_product(db, d:DataScraps, constData:Product):
    
    curTierPrice = [TierPrice(Price=d.price).__dict__]
    
    product_card = Product(ProductTypeId = constData.ProductTypeId,
                            ParentGroupedProductId = constData.ParentGroupedProductId,
                            VisibleIndividually = constData.VisibleIndividually,
                            Name = d.name,
                            ShortDescription = d.url,
                            Url = d.url,
                            ProductTemplateId = constData.ProductTemplateId,
                            VendorId = constData.VendorId,
                            Sku = d.sku,
                            DeliveryDateId = constData.DeliveryDateId,
                            TaxCategoryId = constData.TaxCategoryId,
                            WarehouseId = constData.WarehouseId,
                            Price = d.price,
                            OldPrice = d.oldprice,
                            UnitId = constData.UnitId,
                            TierPrices = curTierPrice
                           )
    product_card.SeName = get_sename(d.sku, db, product_card._id, 'Product', '')

    return product_card

def get_sename(sename, db, entityId = None, entityName = None, languageId = None):
        #replacechar = [' ', '-', '!', '?', ':', '"', '.', '+']
        okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-"
        okRuChars = "abcdefghijklmnopqrstuvwxyzабвгдеёжзийлкмнопрстуфхцчшщъыьэюя1234567890 _-"
        senamenew_ru = sename.lower().replace('і', 'i')
        senamenew_ru = senamenew_ru.replace('є', 'e')
        senamenew_ru = senamenew_ru.replace('ї', 'i')
        senamenew_ru = senamenew_ru.replace('"', '_inch_')
        senamenew_ru = senamenew_ru.replace('+', '_plus_')
        senamenew_ru = senamenew_ru.replace('-', '_minus_')
        sename_new = ''

        for s in senamenew_ru:
            if okRuChars.__contains__(s):
                sename_new = sename_new + s

        senamenew_ru = sename_new

        senamenew_ru = pytils.translit.translify(senamenew_ru.strip()).lower()

        sename_new = ''
        for s in senamenew_ru:
            if okChars.__contains__(s):
                sename_new = sename_new + s

        sename_new = sename_new.replace(' ', '-')
        sename_new = sename_new.replace('--', '-')
        sename_new = sename_new.replace('__', '_')

        if not entityId is None:
            check_slug(sename_new, db, entityId, entityName, languageId)
        return sename_new

def check_slug(slug, db, entityId, entityName, languageId):

    if find_document(db.UrlRecord, {'EntityId': entityId, 'EntityName': entityName, 'LanguageId': languageId}) is None:
        new_v = UrlRecord(EntityId=entityId, 
                          EntityName=entityName, 
                          Slug=slug, 
                          LanguageId=languageId)

        insert_document(db.UrlRecord, new_v.__dict__)

def load_image(url,filename):
    if not os.path.exists(filename):
        urlretrieve(url, filename)

def clear_all_product(db):
    products = find_document(db.Product, {}, multiple = True)
    for product in products:
        pictures = product.get('ProductPictures')
        pictures = [] if pictures is None else pictures
        SeName = product.get('SeName')

        for picture in pictures:
            delete_document(db.Picture, {'_id': picture.get('PictureId')})
            filename = get_file_picture_name(picture.get('PictureId'))
            if os.path.exists(filename):
                os.remove(filename)

        delete_document(db.Product, {'_id': product.get('_id')})
        delete_document(db.UrlRecord, {'EntityName': 'Product', 'Slug': SeName})
        print('Deleted product id = {0}',product.get('_id'))

    ads = find_document(db.Ad, {}, multiple = True)
    for ad in ads:
        delete_document(db.Ad, {'_id': ad.get('_id')})

    PrivateMessages = find_document(db.PrivateMessage, {}, multiple = True)
    for pm in PrivateMessages:
        delete_document(db.PrivateMessage, {'_id': pm.get('_id')})