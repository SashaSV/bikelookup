from marshmallow import Schema, SchemaOpts, fields, post_load
#from modeldb import Category
from dataclasses import dataclass
from bson.objectid import ObjectId

class CategoryOptions(SchemaOpts):
    """ Override schema default options. """

    def __init__(self, meta):
        super(CategoryOptions, self).__init__(meta)
        self.dateformat = '%Y-%m-%d %H:%M:%S'
        self.strict = True

    def __post_init__(self):
        self._id = str(ObjectId())

@dataclass
class Locale:
    _id: str=None
    LanguageId:str=None
    LocaleKey:str=None
    LocaleValue:str=None

    def __post_init__(self):
        self._id = str(ObjectId())

class LocaleSchema(Schema):
    _id = fields.Str()
    LanguageId = fields.Str(allow_none=True)
    LocaleKey = fields.Str(allow_none=True)
    LocaleValue = fields.Str(allow_none=True)
    
    @post_load
    def make_locale(self, data, **kwargs):
        return Locale(**data)

#@dataclass
class CategorySpecificationAttribute:
    # _id: str=None
    # GenericAttributes = []
    # CategoryId:str=None
    # AttributeTypeId:int=0
    # SpecificationAttributeId:str=None
    # DetailsUrl:str=None
    # SpecificationAttributeOptionId:str=None
    # CustomValue:str=None
    # AllowFiltering: bool=False
    # ShowOnProductPage: bool=False
    # DisplayOrder:int=0
    # Locales = []
    # AttributeType:int=0

    #def __post_init__(self):
    def __init__(self, _id: str=None
                 , GenericAttributes = []
                 , CategoryId:str = None
                 , AttributeTypeId:int=0
                 , SpecificationAttributeId:str=None
                 , DetailsUrl:str=None
                 , SpecificationAttributeOptionId:str=None
                 , CustomValue:str=None
                 , AllowFiltering: bool=False
                 , ShowOnProductPage: bool=False
                 , DisplayOrder:int=0
                 , Locales = []
                 , AttributeType:int=0):
        self._id = _id or str(ObjectId())
        self.GenericAttributes = GenericAttributes
        self.CategoryId = CategoryId
        self.AttributeTypeId = AttributeTypeId
        self.SpecificationAttributeId = SpecificationAttributeId
        self.DetailsUrl = DetailsUrl
        self.SpecificationAttributeOptionId = SpecificationAttributeOptionId
        self.CustomValue = CustomValue
        self.AllowFiltering = AllowFiltering
        self.ShowOnProductPage = ShowOnProductPage
        self.DisplayOrder = DisplayOrder
        self.Locales = []
        self.AttributeType = AttributeType
        #self.GenericAttributes = []
        #self.Locales = []
        
class CategorySpecificationAttributeSchema(Schema):
    _id = fields.Str()
    GenericAttributes = fields.List(fields.Str(allow_none=True))
    CategoryId = fields.Str(allow_none=True)
    AttributeTypeId = fields.Int()
    SpecificationAttributeId = fields.Str(allow_none=True)
    DetailsUrl = fields.Str(allow_none=True)
    SpecificationAttributeOptionId = fields.Str(allow_none=True)
    CustomValue = fields.Str(allow_none=True)
    AllowFiltering = fields.Bool()
    ShowOnProductPage = fields.Bool()
    DisplayOrder = fields.Int()
    Locales = fields.List(fields.Nested(LocaleSchema))
    AttributeType = fields.Int()

    @post_load
    def make_categorySpecificationAttribute(self, data, **kwargs):
        return CategorySpecificationAttribute(**data)

from datetime import datetime
from datetime import timezone

class Category:
     def __init__(self
                , _id: str = None
                , GenericAttributes=[]
                , Name: str=None
                , SeName: str=None
                , Description: str=None
                , BottomDescription: str=None
                , CategoryTemplateId: str=None
                , MetaKeywords=None
                , MetaDescription: str=None
                , MetaTitle: str=None
                , ParentCategoryId: str=None
                , PictureId: str=None
                , PageSize: int=20
                , AllowCustomersToSelectPageSize: bool=False
                , PageSizeOptions: str="6 3 9"
                , PriceRanges: float=None
                , ShowOnHomePage: bool=False
                , FeaturedProductsOnHomaPage: bool=False
                , ShowOnSearchBox: bool=False
                , SearchBoxDisplayOrder: int=0
                , IncludeInTopMenu: bool=False
                , SubjectToAcl: bool=False
                , CustomerRoles=[]
                , Stores= []
                , LimitedToStores: bool=False
                , Published: bool=True
                , DisplayOrder: int=0
                , Flag: str=None
                , FlagStyle: str=None
                , Icon: str=None
                , Active: bool=False
                , Deleted: bool=False
                , DefaultSort: int=5
                , HideOnCatalog: bool=False
                , CreatedOnUtc: datetime = datetime.now()
                , UpdatedOnUtc: datetime = datetime.now()
                , AppliedDiscounts=[]
                , CategorySpecificationAttributes= []
                , Locales= []):
        self._id = _id
        self.GenericAttributes=GenericAttributes
        self.Name = Name
        self.SeName = SeName
        self.Description = Description
        self.BottomDescription = BottomDescription
        self.CategoryTemplateId = CategoryTemplateId
        self.MetaKeywords = MetaKeywords
        self.MetaDescription = MetaDescription
        self.MetaTitle = MetaTitle
        self.ParentCategoryId = ParentCategoryId
        self.PictureId = PictureId
        self.PageSize = PageSize
        self.AllowCustomersToSelectPageSize = AllowCustomersToSelectPageSize
        self.PageSizeOptions = PageSizeOptions
        self.PriceRanges = PriceRanges
        self.ShowOnHomePage = ShowOnHomePage
        self.FeaturedProductsOnHomaPage = FeaturedProductsOnHomaPage
        self.ShowOnSearchBox = ShowOnSearchBox
        self.SearchBoxDisplayOrder = SearchBoxDisplayOrder
        self.IncludeInTopMenu = IncludeInTopMenu
        self.SubjectToAcl = SubjectToAcl
        self.CustomerRoles=CustomerRoles
        self.Stores= Stores
        self.LimitedToStores = LimitedToStores
        self.Published = Published
        self.DisplayOrder = DisplayOrder
        self.Flag = Flag
        self.FlagStyle = FlagStyle 
        self.Icon = Icon
        self.Active = Active
        self.Deleted = Deleted
        self.DefaultSort = DefaultSort
        self.HideOnCatalog = HideOnCatalog
        self.CreatedOnUtc = CreatedOnUtc
        self.UpdatedOnUtc = UpdatedOnUtc
        self.AppliedDiscounts = AppliedDiscounts
        self.CategorySpecificationAttributes = CategorySpecificationAttributes
        self.Locales= Locales

@dataclass
class Category2:
    _id: str=None
    GenericAttributes=[]
    Name: str=None
    SeName: str=None
    Description: str=None
    BottomDescription: str=None
    CategoryTemplateId: str=None
    MetaKeywords=None
    MetaDescription: str=None
    MetaTitle: str=None
    ParentCategoryId: str=None
    PictureId: str=None
    PageSize: int=20
    AllowCustomersToSelectPageSize: bool=False
    PageSizeOptions: str="6 3 9"
    PriceRanges: float=None
    ShowOnHomePage: bool=False
    FeaturedProductsOnHomaPage: bool=False
    ShowOnSearchBox: bool=False
    SearchBoxDisplayOrder: int=0
    IncludeInTopMenu: bool=False
    SubjectToAcl: bool=False
    CustomerRoles=[]
    Stores= []
    LimitedToStores: bool=False
    Published: bool=True
    DisplayOrder: int=0
    Flag: str=None
    FlagStyle: str=None
    Icon: str=None
    Active: bool=False
    Deleted: bool=False
    DefaultSort: int=5
    HideOnCatalog: bool=False
    CreatedOnUtc: datetime = datetime.now(tz = timezone.utc)
    UpdatedOnUtc: datetime = datetime.now(tz = timezone.utc)
    AppliedDiscounts=[]
    CategorySpecificationAttributes= []
    Locales= []

    def __post_init__(self):
        self._id = str(ObjectId())
        #self.CreatedOnUtc = datetime.now()
        #self.UpdatedOnUtc = datetime.now()

class CategorySchema(Schema):
    _id = fields.Str()
    GenericAttributes = fields.List(fields.Str())
    Name = fields.Str()
    SeName = fields.Str()
    Description = fields.Str(allow_none=True)
    BottomDescription = fields.Str(allow_none=True)
    CategoryTemplateId = fields.Str()
    MetaKeywords = fields.Str(allow_none=True)
    MetaDescription = fields.Str(allow_none=True)
    MetaTitle = fields.Str(allow_none=True)
    ParentCategoryId = fields.Str(allow_none=True)
    PictureId = fields.Str(allow_none=True)
    PageSize = fields.Int()
    AllowCustomersToSelectPageSize = fields.Bool()
    PageSizeOptions = fields.Str()
    PriceRanges = fields.Decimal(allow_none=True)
    ShowOnHomePage = fields.Bool()
    FeaturedProductsOnHomaPage = fields.Bool()
    ShowOnSearchBox = fields.Bool()
    SearchBoxDisplayOrder = fields.Int()
    IncludeInTopMenu = fields.Bool()
    SubjectToAcl = fields.Bool()
    CustomerRoles = fields.List(fields.Str())
    Stores = fields.List(fields.Str())
    LimitedToStores = fields.Bool()
    Published = fields.Bool()
    DisplayOrder = fields.Int()
    Flag = fields.Str(allow_none=True)
    FlagStyle = fields.Str(allow_none=True)
    Icon = fields.Str(allow_none=True)
    DefaultSort = fields.Int()
    HideOnCatalog = fields.Bool()
    CreatedOnUtc = fields.DateTime()
    #fields.Function(lambda obj: obj.CreatedOnUtc.isoformat())
    UpdatedOnUtc = fields.DateTime(
                        #dump_only=True,
                        default=lambda: datetime.now(tz = timezone.utc),
                        missing=lambda: datetime.now(tz = timezone.utc),
                        allow_none=False)
    AppliedDiscounts = fields.List(fields.Str())
    CategorySpecificationAttributes = fields.List(fields.Nested(CategorySpecificationAttributeSchema))
    Locales = fields.List(fields.Nested(LocaleSchema))
    Active = fields.Bool()
    Deleted = fields.Bool()

    @post_load
    def make_category(self, data, **kwargs):
        return Category(**data)
    
import json
import os

CURR_DIR = os.getcwd()
if CURR_DIR.find("Python.Script") < 0 :
    CURR_DIR = '{0}\\Python.Script'.format(CURR_DIR)

def dump_to_json(filename, data, **kwargs):
    kwargs.setdefault('ensure_ascii', False)
    kwargs.setdefault('indent', 1)
    
    filename = '{0}\\{1}'.format(CURR_DIR,filename)
    
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(data, f, **kwargs, sort_keys=True, default=str)
    
    return json.dumps(data, indent=4, sort_keys=True, default=str)