# -*- coding: utf-8 -*-
import json
import urllib
import os
import posixpath
import xlsxwriter
import requests

from bs4 import BeautifulSoup
from pymongo import MongoClient

#86
NAMEMACHINE = 'localhost'
PORTDB = 27017
NAMEDB = 'bludb1'
PAGES_START = 1
PAGES_COUNT = 1
OUT_FILENAME = 'velogo'
OUT_XLSX_FILENAME = 'velogo'
VENDOR = 'velogo.com.ua'
HOST = 'https://velogo.com.ua'
HEADERS = {'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:71.0) Gecko/20100101 Firefox/71.0',
           'accept': '*/*'}
jsonFileNameOfColorBase = 'colorbase.json'

hosts = [
    {
        'isPicture': True,
        'isTechs': True,
        'parsName': True,
        'urlsite': 'https://velogo.com.ua',
        'FMT': 'https://velogo.com.ua/velo?per_page={page}',
        'PAGES_COUNT': 1,
        'OUT_FILENAME': 'velogo',
        'VENDOR': 'velogo.com.ua',
        'COUNTONPAGE': 24,
        'FINDURL': 'soup.find_all("a", class_="product-cut__title-link")',
        'AVAILABLEURL': "tag.find('span', class_='product-photo__not_available product-photo__not_available--size_c hidden')",
        "HREF": "tag.attrs['href']",
        'NAME': "soup.find('h1', class_='content__title')",
        "BRANDOBJIMG": "soup.find('img', class_='brands-widget__item')",
        "BRANDOBJTEXT": "soup.find('span', class_='brands-widget__item')",
        "TECHLIST": "soup.find_all('div', class_='properties__item')",
        "NAMEOPTION": "row.find('span', class_='tooltip__label')",
        "VALUEOPTION": "row.find('div', class_='properties__value')",
        "IMAGELIST": "soup.find_all('a', class_='product-photo__thumb-item')",
        "OLDPRICE": "soup.find('span', class_='product-price__old-value')",
        "BASEPRICE": "soup.find('div', class_='product-intro__price row')",
        "OLDPRICEVAL": "op.get_text(strip=True)",
        "PRICE": "soup.find('span', class_='product-price__main-value')",
        "PRICEVAL": "p.get_text(strip=True)",
        "SKUCARDOPTION": "soup.find('input', class_='delivery-radio__check')",
        "SKU": "cardoption.get('data-product-variant--number').strip()",
        "SKUBASE": "soup.find('div', class_='product-intro__addition-item')",
        "SIZE": "param.find('span', class_='variants-radio__field-inner').find_all('span', class_='variants-radio__title')[1].get_text(strip=True)"
    },
    {
        'isPicture': True,
        'isTechs': True,
        'parsName': True,
        'urlsite': 'https://bike-family.com.ua',
        'FMT': 'https://bike-family.com.ua/velosipedy?user_per_page=48&per_page={page}/',
        'PAGES_COUNT': 6,
        'OUT_FILENAME': 'bike-family',
        'VENDOR': 'bike-family.com.ua',
        'COUNTONPAGE': 48,
        'FINDURL': "soup.find_all('div', class_='col-xs-6 col-sm-6 col-md-3 col-lg-3')",
        'AVAILABLEURL': "tag.find('div', class_='product-buy__unavailable hidden').get('data-product-unavailable')",
        "HREF": "tag.find('a', class_='product-cut__title-link').get('href')",
        'NAME': "soup.find('h1', class_='content__title')",
        "BRANDOBJIMG": "None",
        "BRANDOBJTEXT": "soup.find('a', class_='product-intro__addition-link')",
        "TECHLIST": "soup.find_all('div', class_='properties__item')",
        "NAMEOPTION": "row.find('span', class_='tooltip__label')",
        "VALUEOPTION": "row.find('div', class_='properties__value')",
        "IMAGELIST": "soup.find_all('a', class_='product-photo__thumb-item')",
        "BASEPRICE": "soup.find('div', class_='product-intro__price')",
        "OLDPRICE": "soup.find('div', class_='product-price__old')",
        "OLDPRICEVAL": "op.find('span', class_='product-price__item-value').get_text(strip=True)",
        "PRICE": "soup.find('div', class_='product-price__main')",
        "PRICEVAL": "p.find('span', class_='product-price__item-value').get_text(strip=True)",
        "SKUCARDOPTION": "soup.find('input', class_='variants-radio__control')",
        "SKU": "cardoption.get('data-product-variant--number').strip()",
        "SKUBASE": "soup.find('div', class_='product-intro__addition')",
        "SIZE": "param.find('span', class_='variants-radio__title').get_text(strip=True)"
    }
    ]
OUT_FILE_CATALOG = 'C://Users/SashaSV/source/repos/bikelookup/Grand.Web/wwwroot/content/images/'

_separetAtribute = ['sp_for', 'sp_type_brake', 'sp_typebike', 'sp_color']
_separeteAtributeWithLinq = ["sp_equipment", "sp_model"]

_filteringAtribute = ["manufacturer", "sp_size", "sp_available", "sp_typebike", "sp_for", "sp_material_frame",
                      "sp_wheeldiams", "sp_type_brake", "sp_year", "sp_count_speed", "sp_type_fork",
                      "sp_type_rear_hub", "sp_age", "sp_fork_travel", "sp_equipment",
                      "sp_model", "sp_color"]

_nonVisibleAtribute = ["sp_available", "sp_brand_fork"]
_visibleOnSellerTabAtribute = ["sp_year", "manufacturer", "sp_for", "sp_count_speed", "sp_size", "sp_material_frame", "sp_type_fork", "sp_equipment", "sp_type_brake"]


SPECNAMEBASE = {
    #general/общая
    'manufacturer':
        {'site': 'Производитель', 'ru': 'Производитель', 'ua': 'Выробник', 'en': 'Manufacturer',
         'displayOrderOnTabProduct': 1,
         'displayOrderOnTabFilter': 9},
    'sp_available': {'site': 'Доступность', 'ru': 'Доступность', 'ua': 'Доступність', 'en': 'Available',
         'displayOrderOnTabProduct': 1,
         'displayOrderOnTabFilter': 1},
    'sp_year': {'site': ['Модельный год', 'Год'], 'ru': 'Модельный год', 'ua': 'Модельний рік', 'en': 'Year',
         'displayOrderOnTabProduct': 1,
         'displayOrderOnTabFilter': 3},
    'sp_typebike': {'site': 'Тип велосипеда', 'ru': 'Тип велосипеда', 'ua': 'Тип велосипеда', 'en': 'Type bike',
         'displayOrderOnTabProduct': 1,
         'displayOrderOnTabFilter': 5},
    'sp_for': {'site': 'Для кого', 'ru': 'Для кого', 'ua': 'Для кого', 'en': 'For',
         'displayOrderOnTabProduct': 3,
         'displayOrderOnTabFilter': 7},
    'sp_age': {'site': 'Возраст (Рост)', 'ru': 'Возраст', 'ua': 'Вік', 'en': 'Age',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 15},
    'sp_height': {'site': 'Рост', 'ru': 'Рост', 'ua': 'Ріст', 'en': 'Height',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 99},
    'sp_size': {'site': 'Размер рамы', 'ru': 'Размер рамы', 'ua': 'Розмір рами', 'en': 'Frame size',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 12},
    'sp_model': {'site': ['Модельный ряд', 'Модель'], 'ru': 'Модельный ряд', 'ua': 'Модельний ряд', 'en': 'Model',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 10},
    'sp_color': {'site': 'Цвет', 'ru': 'Цвет', 'ua': 'Колір', 'en': 'Color',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 30},

    #transmission/трансмиссия
    'sp_equipment':
        {'site': 'Оборудование',
         'ru': 'Оборудование',
         'ua': 'Обладнання',
         'en': 'Equipment',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 30},
    'sp_count_speed':
        {'site': 'Количество скоростей',
         'ru': 'Количество скоростей',
         'ua': 'Кількість швидкостей',
         'en': 'Count Speed',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 40},
    'sp_shift':
        {'site': 'Ручки переключения (манетки, дуалы)',
         'ru': 'Ручки переключения',
         'ua': 'Ручки перемикання',
         'en': 'Shifters'},
    'sp_front_shifter':
        {'site': 'Передний переключатель',
         'ru': 'Передний переключатель',
         'ua': 'Передній перемикач',
         'en': 'Front shifter'},
    'sp_rear_shifter':
        {'site': 'Задний переключатель',
         'ru': 'Задний переключатель',
         'ua': 'Задній перемикач',
         'en': 'Rear shifter'},
    'sp_crankset':
        {'site': 'Система (шатуны)',
         'ru': 'Система (шатуны)', 'ua': 'Система (шатуни)', 'en': 'Сrankset'},
    'sp_cassette':
        {'site': 'Задние звезды',
         'ru': 'Задние звезды', 'ua': 'Задні зірки', 'en': 'Cassette'},
    'sp_chain':
        {'site': 'Цепь', 'ru': 'Цепь', 'ua': 'Ланцюг', 'en': 'Chain'},

    #frameset/рама
    'sp_material_frame': {'site': 'Материал рамы', 'ru': 'Материал рамы', 'ua': 'Матеріал рами', 'en': 'Material frame',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 20},
    'sp_frame': {'site': 'Рама', 'ru': 'Рама', 'ua': 'Рама', 'en': 'Frame'},
    'sp_type_fork': {'site': 'Тип вилки', 'ru': 'Тип вилки', 'ua': 'Тип вилки', 'en': 'Type fork',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 22},
    'sp_brand_fork': {'site': 'Бренд вилки',
                      'ru': 'Производитель вилки',
                      'ua': 'Виробник вилки',
                      'en': 'Brand fork'},
    'sp_fork': {'site': 'Вилка', 'ru': 'Вилка', 'ua': 'Вилка', 'en': 'fork'},
    'sp_fork_travel': {'site': 'Ход вилки', 'ru': 'Ход вилки', 'ua': 'Хід вилки', 'en': 'Fork travel',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 24},
    'sp_seatpost':
        {'site': 'Подседельная труба (глагол)',
         'ru': 'Подседельная труба',
         'ua': 'Підседельна труба',
         'en': 'Seatpost'},
    'sp_headset': {'site': 'Рулевая колонка',
                   'ru': 'Рулевая колонка',
                   'ua': 'Рульова колонка',
                   'en': 'Headset'},
    'sp_carriage': {'site': 'Каретка', 'ru': 'Каретка', 'ua': 'Каретка', 'en': 'Carriage'},
    'sp_rear_shock_absorber':
        {'site': 'Задний амортизатор',
        'ru': 'Задний амортизатор',
        'ua': 'Задній амортизатор',
        'en': 'Rear shock absorber'},

    #brakes/тормоза
    'sp_type_brake': {'site': 'Тип тормозов', 'ru': 'Тип тормозов', 'ua': 'Тип гальм', 'en': 'Type brake',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 38},
    'sp_front_brake': {'site': 'Передний тормоз', 'ru': 'Передний тормоз', 'ua': 'Переді гальма', 'en': 'Front brake'},
    'sp_rear_brake': {'site': 'Задний тормоз', 'ru': 'Задний тормоз', 'ua': 'Задні гальма', 'en': 'Rear brake'},
    'sp_front_rotor': {'site': 'Ротор передний', 'ru': 'Ротор передний', 'ua': 'Ротор передній', 'en': 'Front rotor'},
    'sp_rear_rotor': {'site': 'Ротор задний', 'ru': 'Ротор задний', 'ua': 'Ротор задній', 'en': 'Rear rotor'},
    'sp_brakes': {'site': 'Тормоза', 'ru': 'Тормоза', 'ua': 'Гальма', 'en': 'Brakes'},
    'sp_brake_levers': {'site': 'Тормозные ручки', 'ru': 'Тормозные ручки', 'ua': 'Гальмівні ручки', 'en': 'Brake levers'},

    #wheels/колеса
    'sp_wheeldiams': {'site': ['Размер колес', 'Размер колеса'],
                      'ru': 'Размер колес',
                      'ua': 'Розмір колес',
                      'en': 'Wheeldiams',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 18},
    'sp_front_hub': {'site': 'Передняя втулка', 'ru': 'Передняя втулка', 'ua': 'Передня втулка', 'en': 'Front hub'},
    'sp_type_rear_hub':
        {'site': 'Тип задней втулки',
         'ru': 'Тип задней втулки',
         'ua': 'Тип задньої втулки',
         'en': 'Type rear hub',
         'displayOrderOnTabProduct': 99,
         'displayOrderOnTabFilter': 39},
    'sp_rear_hub': {'site': 'Задняя втулка', 'ru': 'Задняя втулка', 'ua': 'Задня втулка', 'en': 'Rear hub'},
    'sp_rims': {'site': 'Обод', 'ru': 'Обода', 'ua': 'Обода', 'en': 'Rims'},
    'sp_weels': {'site': 'Колеса', 'ru': 'Колеса', 'ua': 'Колеса', 'en': 'Weels'},
    'sp_hubs': {'site': 'Втулки', 'ru': 'Втулки', 'ua': 'Втулки', 'en': 'Hubs'},
    'sp_tyres': {'site': 'Покрышки', 'ru': 'Покрышки', 'ua': 'Покришки', 'en': 'Tyres'},
    'sp_front_tyre': {'site': 'Покрышка передняя',
                      'ru': 'Покрышка передняя',
                      'ua': 'Покришка передня',
                      'en': 'Front tyre'},
    'sp_rear_tyre': {'site': 'Покрышка задняя',
                     'ru': 'Покрышка задняя',
                     'ua': 'Покришка задня',
                     'en': 'Rear tyre'},
    'sp_spokes': {'site': 'Спицы', 'ru': 'Спицы', 'ua': 'Спиці', 'en': 'Spokes'},

    #other/прочее
    'sp_handlebars': {'site': 'Руль', 'ru': 'Руль', 'ua': 'Руль', 'en': 'Handlebars'},
    'sp_stem': {'site': 'Вынос', 'ru': 'Вынос', 'ua': 'Виніс', 'en': 'stem'},
    'sp_weight_limit': {'site': 'Вес велосипедиста, кг',
                        'ru': 'Вес велосипедиста, кг',
                        'ua': 'Вага велосипедиста, кг',
                        'en': 'Weight Limit'},
    'sp_weight_bike': {'site': 'Вес', 'ru': 'Вес', 'ua': 'Вага', 'en': 'Weight bike'},
    'sp_grips': {'site': 'Ручки руля (грипсы, обмотка)',
                 'ru': 'Ручки руля (грипсы, обмотка)',
                 'ua': 'Ручки руля (гріпси, обмотка)',
                 'en': 'Grips'},
    'sp_pedals': {'site': 'Педали', 'ru': 'Педали', 'ua': 'Педалі', 'en': 'Pedals'},
    'sp_saddle': {'site': 'Седло', 'ru': 'Седло', 'ua': 'Сідло', 'en': 'Saddle'},
    'sp_other': {'site': 'Дополнительно', 'ru': 'Дополнительно', 'ua': 'Додатково', 'en': 'Other'},

    #e-motor/е-мотор
    'sp_motor': {'site': 'Мотор', 'ru': 'Мотор', 'ua': 'Двигун', 'en': 'Motor'},
    'sp_battery': {'site': 'Батарея', 'ru': 'Батарея', 'ua': 'Батарея', 'en': 'Battery'},
    'sp_charger': {'site': 'Зарядное', 'ru': 'Зарядное', 'ua': 'Зарядне', 'en': 'Charger'},
    'sp_display': {'site': 'Дисплей', 'ru': 'Дисплей', 'ua': 'Дисплей', 'en': 'Display'}
}

def get_soup(url, **kwargs):
    response = requests.get(url, headers=HEADERS, **kwargs)
    if response.status_code == 200:
        soup = BeautifulSoup(response.text, features='html.parser')
    else:
        soup = None
    return soup

def get_headers_spec(spec_value):

    ret_name_spec = None
    for sp_ind, sp_name in enumerate(SPECNAMEBASE):
        #print(sp_ind, sp_name, SPECNAMEBASE[sp_name])
        sitename = SPECNAMEBASE.get(sp_name.lower())
        if sitename is None:
            continue
        sitename = sitename.get('site')

        if sp_name.lower() == 'sp_year':
            aaa = 1

        sitenameL = sitename
        if type(sitename) is str:
            sitenameL = []
            sitenameL.append(sitename)

        sitenameL = [r.lower() for r in sitenameL]
        #TODO провериить в нижнем регистре
        if sitenameL.__contains__(spec_value.lower()):
            ret_name_spec = sp_name
            break

    return ret_name_spec

def crawl_products(host):
    """
    Собирает со страниц с 1 по pages_count включительно ссылки на товары.
    :param pages_count:     номер последней страницы с товарами.
    :return:                список URL товаров.
    """

    pages_count = host['PAGES_COUNT']
    urls = []
    fmt = host.get('FMT')
    countpage = host.get('COUNTONPAGE')
    #eval()
    for page_n in range(PAGES_START, PAGES_START + pages_count):
        print('page: {0}, {1}({0}x{2})'.format(page_n, page_n * countpage, countpage))

        page_num = 0 if page_n == 0 else page_n * countpage

        page_url = fmt.format(page=page_num)
        soup = get_soup(page_url)
        if soup is None:
            break

        i = 0

        for tag in eval(host.get('FINDURL')):
            available = eval(host.get('AVAILABLEURL'))
            if available is None:
               continue

            href = eval(host.get('HREF'))
            if (href == 'https://bike-family.com.ua/velosiped-trek-procaliber-97-rage-red-2020'):
                aaa = 0

            url = '{}'.format(href)

            if url.strip() == (HOST.strip()+'/'):
                continue
            urls.append(url)
            i = i + 1
            print('{0}. {1}'.format(i, url))

    return urls

def price_without_space(price_with_spac):

    pp = price_with_spac.split()
    price_without_space = ''.join(pp)

    return price_without_space

def sizemodify(sizevalue):
    retsize = sizevalue

    if sizevalue.strip() != '':
        newsize = sizevalue.strip().replace(' ', '/', 1).split('/')
        if len(newsize) > 1:
            if len(newsize[0]) < 7 and len(newsize[1]) > 0:
                if newsize[1][0] != '(':
                    newsize[1] = '(' + newsize[1] + ')'

            retsize = newsize[0]

            #+ '/' + newsize[1]

    return retsize

def appintodata(data, url, name = '', sku = '', brand = '', category = '', price = '0', oldprice = '0',
                vendor = VENDOR, size = '', available = '', techs = [], images = []):

    deletewords = ['Размер рамы ', 'Размер', 'Рама ', 'рама ', 'зріст', 'см', 'на рост ', 'рост ', '(колеса 27,5")', 'колеса 27,5', 'колеса 27.5']

    for word in deletewords:
        size = size.replace(word, '').strip()

    size = sizemodify(size)

    item = {
        'url': url,
        'name': name,
        'sku': sku,
        'manufacturer': brand,
        'category': category,
        'price': price,
        'oldprice': oldprice,
        'vendor': vendor,
        'sp_size': size,
        'sp_available': available,
        'techs': techs,
        'picture': images
    }

    data.append(item)

def get_pars_name(soup, host):

    name = eval(host.get('NAME'))

    if name is None:
        return ''

    name = name.get_text(strip=True)
    name = name.replace('УЦЕНКА -', '')
    return name

def get_pars_brand(soup, host):

    brandobj = eval(host.get("BRANDOBJIMG"))
    if brandobj is None:
        brandobj = eval(host.get("BRANDOBJTEXT"))
        brand = brandobj.get_text(strip=True)
    else:
        brand = {}
        brand['brand'] = brandobj.get('alt')
        brand['brandimg'] = HOST + brandobj.get('src')
    return brand

def get_pars_category(soup):
    """
    categoreis = soup.find_all('li', class_='breadcrumbs__item')
    category = categoreis[3].get_text(strip=True)
    if (len(categoreis) < 5):
        category = categoreis[2].get_text(strip=True)
    """

    category = 'Велосипеды'
    return category

def get_pars_techs(soup, host):
    techs = {}
    for row in eval(host.get('TECHLIST')):
        nameoption = eval(host.get('NAMEOPTION'))
        nameoption = nameoption.get_text(strip=True)
        valueoption = eval(host.get('VALUEOPTION'))
        valueoption = valueoption.get_text(strip=True)
        nameoption = get_headers_spec(nameoption)

        if nameoption is None:
            continue

        techs[nameoption] = valueoption
    return techs

def get_pars_pictures(soup, host):
    images = []
    i = 0
    for image in eval(host.get('IMAGELIST')):
        i = i + 1
        urlimage = image.get('href')
        if urlimage.find('nophoto') >=0:
            continue
        images.append(urlimage)
    return images

def get_parser_oldprice(soup, host):
    oldprice = 0
    if soup is None:
        return oldprice
    op = eval(host.get("OLDPRICE"))
    if op is None:
        return oldprice

    oldprice = eval(host.get("OLDPRICEVAL"))

    if oldprice is None:
        return 0

    oldprice = int(price_without_space(oldprice))
    return oldprice

def get_parser_price(soup, host):
    price = 0

    if soup is None:
        return price
    p = eval(host.get("PRICE"))
    if not p is None:
        price = eval(host.get("PRICEVAL"))
    price = int(price_without_space(price))
    return price

def get_pars_sku(soup, host, variants = False):

    if variants:
        cardoption = eval(host.get("SKUCARDOPTION"))
        sku = eval(host.get("SKU"))
        return sku

    skubase = eval(host.get("SKUBASE"))
    if not skubase is None:
        sku = skubase.find('span').get_text(strip=True)
    return sku

def get_pars_data(data, url, host, isPicture = False, isTechs = False, isCategory = True, isBrand = True,
                  isName = True, isSku = True):

    soup = get_soup(url)

    if soup is None:
        return data

    name = ''
    if isName:
        # name
        name = get_pars_name(soup, host)

    brand = ''
    if isBrand:
        # brand
        brand = get_pars_brand(soup, host)

    category = ''
    if isCategory:
        # category
        category = get_pars_category(soup)

    techs = {}
    # options
    if isTechs:
        techs = get_pars_techs(soup, host)

    images = []
    # images
    if isPicture:
        images = get_pars_pictures(soup, host)

    sku = ''
    # price, size, available, sku
    paramsku = soup.find_all('div', class_='variants-radio__item')
    deletewordsindiam = ['(700с)']
    if len(paramsku) == 0:
        size = ''

        if isSku:
            # sku
            sku = get_pars_sku(soup, host, variants = False)

        baseprice = eval(host.get('BASEPRICE'))
        price = get_parser_price(baseprice, host)
        oldprice = get_parser_oldprice(baseprice, host)

        if int(price) > 0:
            available = 'В наличии'
        else:
            available = 'Продано'

        diams = techs.get('sp_wheeldiams')
        if not diams is None:
            for word in deletewordsindiam:
                diams = diams.replace(word, '').strip()

            techs['sp_wheeldiams'] = diams

        appintodata(data, url, name=name, sku=sku, brand=brand, category=category, price=price,
                               oldprice=oldprice, vendor=host.get('VENDOR'), size=size, available=available,
                               techs=techs, images=images)
    else:
        # print(data)
        for param in paramsku:

            available = param.find('span', class_='variants-radio__available')

            if available is None:
                available = param.find('a', class_='variants-radio__available')

            if available is None:
                return data

            available = available.get_text(strip=True)

            if ['Нет в наличии', 'Продано'].__contains__(available):
                return data

            size = eval(host.get('SIZE'))
            size = size.replace(',', '.')

            new_techs = {}
            if isTechs:
                new_techs = techs.copy()
                diams = techs.get('sp_wheeldiams')
                if not diams is None:
                    for word in deletewordsindiam:
                        diams = diams.replace(word, '').strip()

                    diams = diams.split(', ')
                    new_techs['sp_wheeldiams'] = diams[0]
                    if (size.find('27,5') > 0 or size.find('27.5') > 0):
                        if (len(diams) > 1):
                            new_techs['sp_wheeldiams'] = diams[1]
                        else:
                            new_techs['sp_wheeldiams'] = '27.5'

            if isSku:
                sku = get_pars_sku(param.find('span', class_='variants-radio__field-inner'), host, variants=True)

            price = get_parser_price(param, host)
            oldprice = get_parser_oldprice(param, host)

            appintodata(data, url, name=name, sku=sku, brand=brand, category=category, price=price,
                               oldprice=oldprice, vendor=host.get('VENDOR'), size=size, available=available,
                               techs=new_techs,images=images)

            # print(data)

    return data

def parse_products(urls, host, isPicture=False, isTechs=False, cntPars=0):
    """
    Парсинг полей:
        название, цена и таблица характеристик
    по каждому товару.
    :param urls:            список URL на карточки товаров.
    :return:                массив спарсенных данных по каждому из товаров.
    """
    data = []
    #urls = ['https://velogo.com.ua/velosiped-28-cannondale-synapse-disc-tiagra-emerald']

    for number, url in enumerate(urls, start=1):
        if cntPars > 0 and number == cntPars+1:
            break
        print('#{num}, product: {url}'.format(num=number, url=url))
        data = get_pars_data(data, url, host, isPicture=isPicture, isTechs=isTechs)
    return data

try:
    from urlparse import urlsplit
    from urllib import unquote
except ImportError: # Python 3
    from urllib.parse import urlsplit, unquote

def url2filename(url):
    """Return basename corresponding to url.

    >>> print(url2filename('http://example.com/path/to/file%C3%80?opt=1'))
    fileÀ
    >>> print(url2filename('http://example.com/slash%2fname')) # '/' in name
    Traceback (most recent call last):
    ...
    ValueError
    """
    urlpath = urlsplit(url).path
    basename = posixpath.basename(unquote(urlpath))
    if (os.path.basename(basename) != basename or
        unquote(posixpath.basename(urlpath)) != basename):
        raise ValueError  # reject '%2f' or 'dir%5Cbasename.ext' on Windows
    return basename

from urllib.request import urlretrieve

def load_image(url,filename):
    if not os.path.exists(filename):
        urlretrieve(url, filename)
        #urlretrieve(url, './')

def dump_to_json(filename, data, **kwargs):
    kwargs.setdefault('ensure_ascii', False)
    kwargs.setdefault('indent', 1)

    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(data, f, **kwargs)

def all_uniq_name(data,name_item):
    uniq_n = []
    for row, item in enumerate(data, start=1):
        for prop_name, prop_value in item[name_item].items():
            if (not uniq_n.__contains__(prop_name)):
                uniq_n.append(prop_name)

    return uniq_n

def max_elem(data, name_item):
    max_len = 0
    max_row = 0
    for row, item in enumerate(data, start=0):
        if (len(item[name_item]) > max_len):
            max_len = len(item[name_item])
            max_row = row
    return max_row

def dump_to_xlsx(filename, data):

    if not len(data):
        return None

    with xlsxwriter.Workbook(filename) as workbook:
        ws = workbook.add_worksheet()
        bold = workbook.add_format({'bold': True})

        headers = ['name', 'url', 'sku', 'manufacturer', 'category', 'price', 'oldprice', 'vendor', 'sp_size', 'sp_available']

        uniq_el = all_uniq_name(data, 'techs')
        headers.extend(uniq_el)

        headers.extend(data[max_elem(data,'picture')]['picture'].keys())

        for col, h in enumerate(headers):
            ws.write_string(0, col, h, cell_format=bold)

        for row, item in enumerate(data, start=1):
            ws.write_string(row, 0, item['name'])
            ws.write_string(row, 1, item['url'])
            ws.write_string(row, 2, item['sku'])
            ws.write_string(row, 3, item['manufacturer'])
            ws.write_string(row, 4, item['category'])
            ws.write_string(row, 5, item['price'])
            ws.write_string(row, 6, item['oldprice'])
            ws.write_string(row, 7, item['vendor'])
            ws.write_string(row, 8, item['sp_size'])
            ws.write_string(row, 9, item['sp_available'])

            for prop_name, prop_value in item['techs'].items():
                col = headers.index(prop_name)
                ws.write_string(row, col, prop_value)

            for prop_name, prop_value in item['picture'].items():
                col = headers.index(prop_name)
                ws.write_string(row, col, prop_value)

import pymongo

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

from bson.objectid import ObjectId
import pytils.translit

def check_slug(slug, db, entityId, entityName, languageId):
    collaction = db.UrlRecord
    v = find_document(collaction, {'EntityId': entityId, 'EntityName': entityName, 'LanguageId': languageId})
    if v is None:
        new_v = {
            "_id": str(ObjectId()),
             "GenericAttributes": [],
             "EntityId": entityId,
             "EntityName": entityName,
             "Slug": slug,
             "IsActive": True,
             "LanguageId": languageId
             }
        insert_document(collaction, new_v)


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

def update_name_in_product(db, collection):
    sa = db.SpecificationAttribute
    #print(product.find_one({'ParentGroupedProductId': {'$ne':'null'}}))
    for p in find_document(collection, {'Url': ''}, True):
        name_elem = pars_name(p['Name'])
        for prop_name, prop_value in name_elem.items():
            if prop_value.strip() != '':
                sao = check_specificationattributeoption_by_name(db, prop_name, prop_value)
            else:
                print('null value {0}, {1}'.format(p['Name'], name_elem))
                continue

            sa = check_specificationattribute_by_name(db, prop_name)
            psao = check_productspecificationattributeoption(db, p, sa, sao)

        name = '{0} {1} {2} {3} {4} Frame {5}'.format(name_elem['Weeldiam'],
                                                     name_elem['ManufactureName'],
                                                     name_elem['Model'],
                                                     name_elem['Year'],
                                                     name_elem['Color'],
                                                     name_elem['Size'])

        id_ = p['_id']
        update_document(collection, {'_id': id_}, name_elem)
        update_document(collection, {'_id': id_}, {'Name': name})

        #print(p['_id'], name_elem, p['Name'])

from datetime import datetime
def chek_so_name(db, name, soname):
    sa = check_specificationattribute_by_name(db, soname)
    soretname = ''
    seret = ''
    for so in sa['SpecificationAttributeOptions']:
        seson = get_sename(so.get('Name'), db)
        sename = get_sename(name, db)
        if sename.find(seson) >= 0:
            if len(seson) > len(seret):
                seret = seson
                soretname = so.get('Name')
    return soretname

def chek_in_colorbase(db, nameIn):
    colors = []

    if os.path.exists(jsonFileNameOfColorBase):
        with open(jsonFileNameOfColorBase, encoding='utf-8') as json_file:
            colors = json.load(json_file)

    if len(colors) == 0:
        sa = find_document(db.SpecificationAttribute, {'Name': 'colorbase'})
        colors = sa.get('SpecificationAttributeOptions')


    retColor = ''
    for color in colors:
        nameColor = color.get('Name')
        if nameColor.lower().strip().split(';').__contains__(nameIn.strip().lower()):
            retColor = nameColor.lower().strip()
            break

    return retColor

def pars_name(db, name_in, size_in = '', wheeldiam_in = '', year_in = '', model_in = '', brandname_in = ''):
    deletewords = ['Электровелосипед', 'BMX', 'Велосипед', '(м)', 'OFFICIAL', 'UA', 'Беговел']

    name = name_in

    if name == 'Велосипед 27,5" Cannondale TRAIL 6 Feminine (2021) mantis':
        aaa = 1

    for word in deletewords:
        name = name.replace(word, '').strip()

    for n in name.split(' '):
        wheeldiam = chek_so_name(db, n, 'sp_wheeldiams')
        if len(wheeldiam) > 0 and wheeldiam[1].isdigit():
            wheeldiam = n
            break

    if len(wheeldiam) == 0:
        wheeldiam = wheeldiam_in

    if len(wheeldiam) == 0:
        wheeldiam = name.split(' ')[0]

    if not wheeldiam[1].isdigit():
        wheeldiam = ''
    else:
        if name.find(wheeldiam) >= 0:
            name = name[len(wheeldiam):len(name)].strip()

        wheeldiam = wheeldiam.replace(',', '.')

    manufactureName = brandname_in
    if len(brandname_in) == 0:
        manufactureName = chek_so_name(db, name.split(' ')[0], 'manufacturer')

    if len(manufactureName) == 0:
        manufactureName = name.split(' ')[0]

    manufactureName = manufactureName.replace('+', '')
    name = name.replace(manufactureName, '').strip()

    year = chek_so_name(db, name, 'sp_year')

    if len(year) == 0:
        curyear = datetime.now().year+1
        y = curyear
        while y >= curyear - 6:
            if name.find(str(y)) > 0:
                year = str(y)
                break
            y = y - 1

    model = model_in

    if year != '':
        if name.find(year) >= 0:
            model = name[0:name.find(year)]
            model = model.replace('(', '').strip()
            model = model.replace('-', ' ').strip()

    if len(model) == 0:
        model = chek_so_name(db, name, 'sp_model')

    if len(year) == 0:
        year = year_in

    name = name.replace(model, '').strip()
    name = name.replace(year, '').strip()

    if model.find('рама') > 0:
        model = model.replace(model[model.find('рама'):len(model)], '').strip()
        model = model.replace('(', '').strip()
        model = model.replace(')', '').strip()

    size = ''
    if len(size_in.strip()) > 0:
        size = size_in
        for w in name.split(' '):
            if w == size_in:
                name = name.replace(size_in, '').strip()

    name = name.replace('()', '').strip()
    for n in name.split(' '):
        color = chek_so_name(db, n, 'sp_color')
        if color == '':
            color = chek_in_colorbase(db, n)
        if color != '':
            if color.strip() != n.strip():
                color = n
                break

    if len(color) > 0:
        name = name.replace(color, '').strip()
    else:
        color = name

    if color == '':
        for c in model.split(' '):
            color = chek_in_colorbase(db, c)
            if color != '':
                color = model[model.find(c):len(model)]
                model = model[0:model.find(c)]
                break
            else:
                for m in c.split('-'):
                    color = chek_so_name(db, m, 'sp_color')
                    if color == '':
                        color = chek_in_colorbase(db, m)
                    if color != '':
                        if color.strip() != m.strip():
                            color = c
                            model = model.replace(c, '').strip()
                            break

    if (not ['+','-','(',')','!','_'].__contains__(name) and name_in.find(name) > name_in.find(manufactureName)
            and name_in.find(name) < name_in.find(color)
            and (year == '' or name_in.find(name) < name_in.find(year))):
        model = name

    if name.rfind('Frame') > 0:
        ss = name[name.rfind('Frame'):len(name)]
        size = name[name.rfind('Frame')+5:len(name)].strip()
        size = size.replace('Не указан', '')
        color = name.replace(ss, '').strip()

    pars_name = {
        'Weeldiam': wheeldiam,
        'ManufactureName': manufactureName,
        'Model': model,
        'Year': year,
        'Color': color,
        'Size': size
    }
    return pars_name

def create_product(db, d, constData, name_elem):

    curTierPrice = [{
        '_id': str(ObjectId()),
        'StoreId': None,
        'CustomerRoleId': None,
        'Quantity': 0,
        'Price': d['price'],
        'StartDateTimeUtc': datetime.now(),
        'EndDateTimeUtc': None}]

    id_ = str(ObjectId())
    product_card = {
                '_id': id_,
                'GenericAttributes': [],
                'ProductTypeId': constData['ProductType'],
                'ParentGroupedProductId': constData['ParentGroupedProductId'],
                'VisibleIndividually': constData['VisibleIndividually'],
                'Name': d['name'],
                'SeName': get_sename(d['name'], db, id_, 'Product', ''),
                'ShortDescription': d['url'],
                'Url': d['url'],
                'ProductTemplateId': constData['productTemplateId'],
                'VendorId': constData['vendorid'],
                'Sku': d['sku'],
                'RecurringCycleLength': 100,
                'Weeldiam': name_elem['Weeldiam'],
                'ManufactureName': name_elem['ManufactureName'],
                'Model': name_elem['Model'],
                'Year': name_elem['Year'],
                'Color': name_elem['Color'],
                'Size': name_elem['Size'],
                'FullDescription': None,
                'AdminComment': None,
                'ShowOnHomePage': False,
                'MetaKeywords': None,
                'MetaDescription': None,
                'MetaTitle': None,
                'AllowCustomerReviews': True,
                'ApprovedRatingSum': 0,
                'NotApprovedRatingSum': 0,
                'ApprovedTotalReviews': 0,
                'NotApprovedTotalReviews': 0,
                'SubjectToAcl': False,
                'CustomerRoles': [],
                'LimitedToStores': False,
                'Stores': [],
                'ExternalId': None,
                'ManufacturerPartNumber': None,
                'Gtin': None,
                'IsGiftCard': False,
                'GiftCardTypeId': 0,
                'OverriddenGiftCardAmount': None,
                'RequireOtherProducts': False,
                'RequiredProductIds': None,
                'AutomaticallyAddRequiredProducts': False,
                'IsDownload': False,
                'DownloadId': None,
                'UnlimitedDownloads': True,
                'MaxNumberOfDownloads': 10,
                'DownloadExpirationDays': None,
                'DownloadActivationTypeId': 0,
                'HasSampleDownload': False,
                'SampleDownloadId': None,
                'HasUserAgreement': False,
                'UserAgreementText': None,
                'IsRecurring': False,
                'RecurringCyclePeriodId': 0,
                'RecurringTotalCycles': 10,
                'IncBothDate': False,
                'Interval': 0,
                'IntervalUnitId': 0,
                'IsShipEnabled': False,
                'IsFreeShipping': False,
                'ShipSeparately': False,
                'AdditionalShippingCharge': 0,
                'DeliveryDateId': constData['deliveryDateId'],
                'IsTaxExempt': False,
                'TaxCategoryId': constData['taxCategoryId'],
                'IsTele': False,
                'ManageInventoryMethodId': 0,
                'UseMultipleWarehouses': False,
                'WarehouseId': constData['warehouseId'],
                'StockQuantity': 0,
                'DisplayStockAvailability': False,
                'DisplayStockQuantity': False,
                'MinStockQuantity': 0,
                'LowStock': True,
                'LowStockActivityId': 0,
                'NotifyAdminForQuantityBelow': 1,
                'BackorderModeId': 0,
                'AllowBackInStockSubscriptions': False,
                'OrderMinimumQuantity': 1,
                'OrderMaximumQuantity': 1000,
                'AllowedQuantities': None,
                'AllowAddingOnlyExistingAttributeCombinations': False,
                'NotReturnable': False,
                'DisableBuyButton': False,
                'DisableWishlistButton': False,
                'AvailableForPreOrder': False,
                'PreOrderAvailabilityStartDateTimeUtc': None,
                'CallForPrice': False,
                'Price': d['price'],
                'OldPrice': d['oldprice'],
                'CatalogPrice': 0,
                'ProductCost': 0,
                'CustomerEntersPrice': False,
                'MinimumCustomerEnteredPrice': 0,
                'MaximumCustomerEnteredPrice': 1000,
                'BasepriceEnabled': False,
                'BasepriceAmount': 0,
                'BasepriceUnitId': None,
                'BasepriceBaseAmount': 0,
                'BasepriceBaseUnitId': None,
                'UnitId': constData['unitId'],
                'CourseId': None,
                'MarkAsNew': True,
                'MarkAsNewStartDateTimeUtc': datetime.now()-timedelta(days=1),
                'MarkAsNewEndDateTimeUtc': datetime.now()+timedelta(days=30),
                'Weight': 0,
                'Length': 0,
                'Width': 0,
                'Height': 0,
                'AvailableStartDateTimeUtc': None,
                'AvailableEndDateTimeUtc': None,
                'StartPrice': 0,
                'HighestBid': 0,
                'HighestBidder': None,
                'AuctionEnded': False,
                'DisplayOrder': 0,
                'DisplayOrderCategory': 0,
                'DisplayOrderManufacturer': 0,
                'Published': True,
                'CreatedOnUtc': datetime.now(),
                'UpdatedOnUtc': datetime.now(),
                'Sold': 0,
                'Viewed': 0,
                'OnSale': 0,
                'Flag': None,
                'Locales': [],
                'ProductCategories': [],
                'ProductManufacturers': [],
                'ProductPictures': [],
                'ProductSpecificationAttributes': [],
                'ProductTags': [],
                'ProductAttributeMappings': [],
                'ProductAttributeCombinations': [],
                'TierPrices': curTierPrice,
                'AppliedDiscounts': [],
                'ProductWarehouseInventory': [],
                'CrossSellProduct': [],
                'RelatedProducts': [],
                'SimilarProducts': [],
                'BundleProducts': []
            }

    return product_card
from datetime import datetime
from datetime import timedelta
def check_vendor(db, vendorName):
    collaction = db.Vendor
    v = find_document(collaction, {'Name': vendorName})
    if v is None:
        id_ = str(ObjectId())
        adresses = {
        '_id':id_,
        'GenericAttributes':[],
        'FirstName': None,
        'LastName':None,
        'Email': None,
        'Company': None,
        'VatNumber': None,
        'CountryId': None,
        'StateProvinceId': None,
        'City': None,
        'Address1': None,
        'Address2': None,
        'ZipPostalCode': None,
        'PhoneNumber': None,
        'FaxNumber': None,
        'CustomAttributes': None,
        'CreatedOnUtc':datetime.now()
        }

        id_ = str(ObjectId())
        new_v = {
             "_id": id_,
             "GenericAttributes": [],
             "Name": vendorName,
             "SeName": get_sename(vendorName, db, id_, 'Vendor', ''),
             "PictureId": None,
             "Email": "sales@"+vendorName,
             "Description": None,
             "StoreId": None,
             "AdminComment": None,
             "Active": True,
             "Deleted": False,
             "DisplayOrder": 0,
             "MetaKeywords": None,
             "MetaDescription": None,
             "MetaTitle": None,
             "PageSize": 20,
             "AllowCustomersToSelectPageSize": True,
             "PageSizeOptions": None,
             "AllowCustomerReviews": True,
             "ApprovedRatingSum": 0,
             "NotApprovedRatingSum": 0,
             "ApprovedTotalReviews": 0,
             "NotApprovedTotalReviews": 0,
             "Commission": 0,
             "Address": adresses,
             "Locales": [],
             "VendorNotes": [],
             "AppliedDiscounts": [],
             "VendorSpecificationAttributes": []
        }
        insert_document(collaction, new_v)
        v = new_v
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
    update_document(db.SpecificationAttribute, {'_id': sa['_id']},
                    {'SpecificationAttributeOptions': sa['SpecificationAttributeOptions']})
    return new_sao

def check_specificationattributeoption_by_name(db, prop_name, prop_value, color_hex = None, parentSPO = None):
    sa = check_specificationattribute_by_name(db, prop_name)
    sao = sa['SpecificationAttributeOptions']

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
        'SpecificationAttributeId': sa['_id'],
        'SpecificationAttributeOptionId': sao['_id'],
        'CustomValue': 'null',
        'AllowFiltering': _filteringAtribute.__contains__(sa['Name']),
        'ShowOnProductPage': not _nonVisibleAtribute.__contains__(sa['Name']),
        'ShowOnSellerPage': _visibleOnSellerTabAtribute.__contains__(sa['Name']),
        'DisplayOrder': 0,
        'Locales': []
    }

    psao.append(new_psao)
    update_document(db.Product, {'_id': p['_id']},
                    {'ProductSpecificationAttributes': psao})
    return psao

def check_productspecificationattributeoption(db, p, sa, sao):
    psao_ret = None
    psao = p['ProductSpecificationAttributes']

    for ind, a in enumerate(psao):
        if a['SpecificationAttributeId'] == sa['_id'] and a['SpecificationAttributeOptionId'] == sao['_id']:
            psao_ret = psao[ind]
            spOption = SPECNAMEBASE.get(sa['Name'])

            do = 99 if spOption.get('displayOrderOnTabProduct') is None else spOption.get('displayOrderOnTabProduct')

            psao[ind]['AllowFiltering'] = _filteringAtribute.__contains__(sa['Name'])
            psao[ind]['ShowOnProductPage'] = not _nonVisibleAtribute.__contains__(sa['Name'])
            psao[ind]['ShowOnSellerPage'] = _visibleOnSellerTabAtribute.__contains__(sa['Name'])
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
        id_ = str(ObjectId())
        new_sa = {
            '_id': id_,
            'GenericAttributes': [],
            'Name': prop_name,
            'SeName': get_sename(prop_name, db, id_, 'SpecificationAttribute', ''),
            'DisplayOrder': 0,
            'Locales': get_locals(db, prop_name),
            'SpecificationAttributeOptions': []
        }
        insert_document(collaction, new_sa)
        sa = new_sa
    else:
        spOption = SPECNAMEBASE.get(prop_name)
        do = 99 if spOption.get('displayOrderOnTabFilter') is None else spOption.get('displayOrderOnTabFilter')
        updExpr ={}
        updExpr['DisplayOrder'] = do
        update_document(collaction, {'_id': sa.get('_id')}, updExpr)
    return sa

def get_locals(db, prop_name):
    collaction = db.Language
    lngs = find_document(collaction, {}, multiple = True)
    locals = []
    for l in lngs:
        name_local = SPECNAMEBASE.get(prop_name)
        if name_local is None:
            continue
        name_local = name_local.get(l['Name'].lower())
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

def check_mainproduct(db, d, constData, name_elem):
    collaction = db.Product
    p_main = find_document(collaction, {'Sku': d['sku'], 'VendorId': ''})

    if p_main == None:
        new_p = create_product(db, d, constData, name_elem)
        new_p['Url'] = ''
        new_p['ShortDescription'] = ''
        new_p['VendorId'] = ''
        insert_document(collaction, new_p)
        p_main = new_p
    else:
        updateExpr = {}
        updateExpr['Name'] = d['name']
        #updateExpr['SeName'] = get_sename(d['name'], db, p_main.get('_id'), 'Product', ''),
        update_document(collaction, {'_id': p_main.get('_id')}, updateExpr)
    check_rel_сategories_to_product(db, p_main, d['category'])
    check_rel_manufacturers_to_product(db, p_main, d['manufacturer'])
    return p_main

def check_сategories(db, c_name):
    collaction = db.Category
    c_main = find_document(collaction, {'Name': c_name})
    if c_main == None:
        categoryTemplateId = find_document(db.CategoryTemplate, {})
        categoryTemplateId = categoryTemplateId['_id'] if not categoryTemplateId is None else None
        id_ = str(ObjectId())
        new_c = {
            '_id': id_,
            'GenericAttributes': [],
            'Name': c_name,
            'SeName': get_sename(c_name, db, id_, 'Category', ''),
            'Description': None,
            'BottomDescription': None,
            'CategoryTemplateId': categoryTemplateId,
            'MetaKeywords': None,
            'MetaDescription': None,
            'MetaTitle': None,
            'ParentCategoryId': '',
            'PictureId': None,
            'PageSize': 20,
            'AllowCustomersToSelectPageSize': False,
            'PageSizeOptions': "6, 3, 9",
            'PriceRanges': None,
            'ShowOnHomePage': False,
            'FeaturedProductsOnHomaPage': False,
            'ShowOnSearchBox': False,
            'SearchBoxDisplayOrder': 0,
            'IncludeInTopMenu': False,
            'SubjectToAcl': False,
            'CustomerRoles': [],
            'Stores': [],
            'LimitedToStores': False,
            'Published': True,
            'DisplayOrder': 0,
            'Flag': None,
            'FlagStyle': None,
            'Icon': None,
            'DefaultSort': 5,
            'HideOnCatalog': False,
            'CreatedOnUtc': datetime.now(),
            'UpdatedOnUtc': datetime.now(),
            'AppliedDiscounts': [],
            'Locales': []
        }
        insert_document(collaction, new_c)
        c_main = new_c
    return c_main

def check_rel_сategories_to_product(db, p, c_name):
    cat_ret = None
    cat = p['ProductCategories']

    c = check_сategories(db, c_name)

    for ind, cr in enumerate(cat):
        if cr['CategoryId'] == c['_id']:
            cat_ret = cat[ind]

    if cat_ret is None:
        cat_ret = add_rel_сategories_to_product(db, p, c, cat)

def check_picture(db, pictureId, urlimage, productname):
    collaction = db.Picture
    p_main = find_document(collaction, {'UrlImage': urlimage})
    if p_main == None:
        id_ = str(ObjectId())
        #filename = OUT_FILE_CATALOG + url2filename(urlimage)
        CURR_DIR = os.getcwd()
        CURR_DIR = CURR_DIR.replace('Python.Script', '')
        file_catalog = '{0}Grand.Web\\wwwroot\\content\\images\\'.format(CURR_DIR)
        try:
            os.stat(file_catalog)
        except:
            os.mkdir(file_catalog)

        filename = '{0}{1}_0.jpeg'.format(file_catalog, id_)
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
            new_p = {
                '_id': id_,
                'GenericAttributes': [],
                'PictureBinary': data,
                'MimeType': 'image/jpeg',
                'SeoFilename': get_sename(productname, db),
                'UrlImage': urlimage,
                'AltAttribute': productname,
                'TitleAttribute': productname,
                'IsNew': True
            }
            insert_document(collaction, new_p)
            p_main = new_p
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
        'CategoryId': c['_id'],
        'IsFeaturedProduct': False,
        'DisplayOrder': 0
    }

    cat.append(new_rel)
    update_document(db.Product, {'_id': p['_id']},
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
        id_ = str(ObjectId())
        new_m = {
            '_id': id_,
            'GenericAttributes': [],
            'Name': brandname,
            'SeName': get_sename(brandname, db, id_, 'Manufacturer', ''),
            'Description': '',
            'BottomDescription': None,
            'ManufacturerTemplateId': manufacturerTemplateId,
            'MetaKeywords': '',
            'MetaDescription': '',
            'MetaTitle': '',
            'ParentCategoryId': '',
            'PictureId': pictureId,
            'PageSize': 20,
            'AllowCustomersToSelectPageSize': True,
            'PageSizeOptions': None,
            'PriceRanges': None,
            'ShowOnHomePage': False,
            'FeaturedProductsOnHomaPage': False,
            'IncludeInTopMenu': False,
            'SubjectToAcl': False,
            'CustomerRoles': [],
            'LimitedToStores': False,
            'Stores': [],
            'Published': True,
            'DisplayOrder': 0,
            'Icon': None,
            'DefaultSort': 5,
            'HideOnCatalog': False,
            'CreatedOnUtc': datetime.now(),
            'UpdatedOnUtc': datetime.now(),
            'AppliedDiscounts': [],
            'Locales': []
        }
        insert_document(collaction, new_m)
        m_main = new_m
    else:
        if not pictureId is None:
            updExpr = {}
            updExpr['PictureId'] = pictureId
            update_document(collaction, {'_id': m_main.get('_id')}, updExpr)
    return m_main

def check_rel_manufacturers_to_product(db, p, m_name):
    mun_ret = None
    man = p['ProductManufacturers']

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

def check_product(db, data):
    collaction = db.Product

    simpleTemplateId = find_document(db.ProductTemplate, {'Name': 'Simple product'})
    simpleTemplateId = simpleTemplateId['_id'] if not simpleTemplateId is None else None
    groupTemplateId = find_document(db.ProductTemplate, {'Name': 'Grouped product (with variants)'})
    groupTemplateId = groupTemplateId['_id'] if not groupTemplateId is None else None
    deliveryDateId = find_document(db.DeliveryDate, {})
    taxCategoryId = find_document(db.TaxCategory, {})
    warehouseId = find_document(db.Warehouse, {})
    unitId = find_document(db.MeasureUnit, {})

    constData = {
            'productTemplateId': simpleTemplateId,
            'deliveryDateId': deliveryDateId['_id'] if not deliveryDateId is None else None,
            'taxCategoryId': taxCategoryId['_id'] if not taxCategoryId is None else None,
            'warehouseId': warehouseId['_id'] if not warehouseId is None else None,
            'unitId': unitId['_id'] if not unitId is None else None,
            'VisibleIndividually': False,
            'ProductType': 5, #singl product
            'vendorid': None,
            'ParentGroupedProductId': None
        }

    manufacturerTemplateId = find_document(db.ManufacturerTemplate, {})
    categoryTemplateId = find_document(db.CategoryTemplate, {})
    cnt = 0
    for d in data:
        vendorid = check_vendor(db, d['vendor'])
        vendorid = vendorid['_id'] if not vendorid is None else None
        p = find_document(collaction, {'Sku': d['sku'], 'VendorId': vendorid})
        constData['vendorid'] = vendorid
        techs = d.get('techs')
        sp_wheeldiams = techs.get('sp_wheeldiams') if not techs is None else techs
        sp_wheeldiams = sp_wheeldiams if not sp_wheeldiams is None else ''
        sp_year = techs.get('sp_year') if not techs is None else techs
        sp_year = sp_year if not sp_year is None else ''
        sp_model = techs.get('sp_model') if not techs is None else techs
        sp_model = sp_model if not sp_model is None else ''
        sp_size = d.get('sp_size')
        sp_size = sp_size if not sp_size is None else ''

        ##
        manufacturer = d['manufacturer']
        brandname = manufacturer if type(manufacturer) is str else manufacturer.get('brand')

        name_elem = pars_name(db, d['name'], size_in=sp_size, wheeldiam_in=sp_wheeldiams, year_in=sp_year, model_in = sp_model, brandname_in = brandname)

        brandname = brandname if name_elem['ManufactureName'].strip() == '' else name_elem['ManufactureName']

        if brandname is None:
            brandname = manufacturer

        name_elem['ManufactureName'] = brandname if name_elem['ManufactureName'].strip() == '' else name_elem['ManufactureName']
        name_elem['Size'] = d['sp_size'] if name_elem['Size'].strip() == '' else name_elem['Size']
        name_elem['Year'] = d['techs'].get('sp_year') if name_elem['Year'].strip() == '' else name_elem['Year']
        name_elem['Year'] = '' if name_elem['Year'] is None else name_elem['Year']

        name = '{0}{1}{2}{3}{4}{5}'.format(
                name_elem['Weeldiam']+' ' if name_elem['Weeldiam'].strip() != '' else '',
                name_elem['ManufactureName']+' ' if name_elem['ManufactureName'].strip() != '' else '',
                name_elem['Model']+' ' if name_elem['Model'].strip() != '' else '',
                name_elem['Year']+' ' if name_elem['Year'].strip() != '' else '',
                name_elem['Color']+' ' if name_elem['Color'].strip() != '' else '',
                ' Frame {0}'.format(name_elem['Size']) if name_elem['Size'].strip() != '' else '')

        name = name.strip()
        name_elem['Model'] = name_elem['ManufactureName']+'->'+name_elem['Model']
        cnt = cnt + 1
        print('{0}. Загрузка {1}'.format(cnt, name))

        for prop_name, prop_value in name_elem.items():
            dicription = {'manufacturename': 'manufacturer',
                          'size': 'sp_size',
                          'year': 'sp_year',
                          'model': 'sp_model',
                          'color': 'sp_color',
                          'weeldiam': 'sp_wheeldiams'}

            prop_name = dicription.get(prop_name.lower()) if dicription.get(prop_name.lower()) else prop_name.lower()
            d['techs'][prop_name.lower()] = prop_value

        d['techs']['sp_available'] = d['sp_available']

        if d['category'] != 'Велосипеды':
            d['techs']['category'] = d['category']

        constData['productTemplateId'] = groupTemplateId
        constData['VisibleIndividually'] = True
        constData['ProductType'] = 10
        d['name'] = name.strip()
        p_main = check_mainproduct(db, d, constData, name_elem)

        if p == None:
            constData['productTemplateId'] = groupTemplateId
            constData['VisibleIndividually'] = False
            constData['ProductType'] = 5
            new_p = create_product(db, d, constData, name_elem)
            new_p['ParentGroupedProductId'] = p_main['_id']
            new_p['Name'] = '{0} - {1}'.format(name, d['vendor'])

            insert_document(collaction, new_p)
            p = new_p


        check_rel_сategories_to_product(db, p, d['category'])
        check_rel_manufacturers_to_product(db, p, manufacturer)

        sao = check_specificationattributeoption_by_name(db, 'sp_available', d.get('sp_available'))
        sa = check_specificationattribute_by_name(db, 'sp_available')
        check_productspecificationattributeoption(db, p, sa, sao)

        print('     Подгрузка характеристик')
        for prop_name, prop_value in d['techs'].items():
            if prop_value.strip() == '':
                continue

            if prop_name == 'sp_wheeldiams':
                prop_value = prop_value.strip() + '"' if not prop_value.__contains__('"') else prop_value

            sa = check_specificationattribute_by_name(db, prop_name)

            if _separetAtribute.__contains__(prop_name) or _separeteAtributeWithLinq.__contains__(prop_name):
                ifWithLinq = _separeteAtributeWithLinq.__contains__(prop_name)
                parentSPO = None

                if ifWithLinq:
                    prop_value = prop_value.replace('->', 'separate')

                prop_value = prop_value.replace(',', 'separate')
                prop_value = prop_value.replace('/', 'separate')
                if prop_name == 'sp_color':
                    prop_value = prop_value.lower().replace('-', 'separate')
                    prop_value = prop_value.lower().replace(' с ', 'separate')
                    prop_value = prop_value.lower().replace('(', ' ').strip()
                    prop_value = prop_value.lower().replace(')', '')

                porps_val = prop_value.split('separate')
                for pv in porps_val:
                    pv = pv.lower().strip()
                    if pv == '':
                        continue
                    colorHex = None
                    if prop_name == 'sp_color':
                        for elementColor in pv.split(' '):
                            if elementColor == 'черный':
                                aaaa =1

                            okRuChars = "абвгдеёжзийлкмнопрстуфхцчшщъыьэюя"
                            if len(elementColor) < 2:
                                continue

                            if okRuChars.__contains__(elementColor[-1]):

                                sklonenie = {'о': 'ый', 'м': 'й', 'е': 'ий'}

                                elementColor = elementColor \
                                    if sklonenie.get(elementColor[-1]) is None \
                                    else elementColor[0:-1]+sklonenie.get(elementColor[-1])

                            color = get_colorbase(db, elementColor)
                            if not color is None:
                                pv = color.get('Name').split(';')[0]
                                colorHex = color.get('ColorSquaresRgb')
                            else:
                                pv = 'Цвет не определен'
                                colorHex = None

                    sao = check_specificationattributeoption_by_name(db, prop_name, pv, color_hex=colorHex, parentSPO=parentSPO)
                    check_productspecificationattributeoption(db, p_main, sa, sao)
                    parentSPO = sao.get('_id') if ifWithLinq else None

                continue
            sao = check_specificationattributeoption_by_name(db, prop_name, prop_value)
            sa = check_specificationattribute_by_name(db, prop_name)
            check_productspecificationattributeoption(db, p_main, sa, sao)

        if len(p_main['ProductPictures']) == 0:
            print('     Подгрузка картинок')
            for urlimage in d['picture']:
                check_image_to_product(db, p_main, urlimage)
    return p

def add_new_price(tierPrices, newPrice):
    id_ = str(ObjectId())
    newTierPrice = {
        '_id': id_,
        'StoreId': None,
        'CustomerRoleId': None,
        'Quantity': 0,
        'Price': newPrice,
        'StartDateTimeUtc': datetime.now(),
        'EndDateTimeUtc': None}

    for tp in tierPrices:
        if tp.get('EndDateTimeUtc') is None:
            tp['EndDateTimeUtc'] = datetime.now() - timedelta(minutes = 1)

    tierPrices.append(newTierPrice)

    return tierPrices
def update_productatribute_by_name(db, psa, atributeName, newAtributeOption):

    sa = check_specificationattribute_by_name(db, atributeName)
    sao = check_specificationattributeoption_by_name(db, atributeName, newAtributeOption)
    for curPsa in psa:
        if sa['_id'] == curPsa['SpecificationAttributeId']:
            if sao['_id'] != curPsa['SpecificationAttributeOptionId']:
                curPsa['SpecificationAttributeOptionId'] = sao['_id']

    return psa

def update_price_in_product_card(db, data, productDb):
    curPrice = productDb.get('Price')
    newPrice = data.get('price')
    updExpr = {}
    collection = db.Product
    if curPrice != newPrice:
        tierPrice = add_new_price(productDb.get('TierPrices'), newPrice)
        updExpr['Price'] = newPrice
        updExpr['TierPrices'] = tierPrice

    if productDb.get('OldPrice') != data.get('oldprice'):
        updExpr['OldPrice'] = data.get('oldprice')

    psa = productDb.get('ProductSpecificationAttributes')
    updExpr['ProductSpecificationAttributes'] = update_productatribute_by_name(db, psa, 'sp_available', data.get('sp_available'))

    updExpr['Published'] = newPrice != 0

    if len(updExpr) > 0:
        update_document(collection, {'_id': productDb.get('_id')}, updExpr)

def check_main_product_without_price(db):
    product = db.Product
    productCards = find_document(product, {'Url': ''}, multiple=True,
                      retfields={'_id'})
    updExpr = {}
    for pCard in productCards:
        pChildCards = find_document(product, {'ParentGroupedProductId': pCard.get('_id'), 'Published': True}, multiple=True,
                      retfields={'_id'})

        updExpr['Published'] = len(pChildCards) > 0 if True else False
        update_document(product, {'_id': pCard.get('_id')}, updExpr)

def check_actual_price_and_available(db):
    product = db.Product

    p = find_document(product, {'Url': {'$ne': ''}}, multiple=True,
                      retfields={'_id', 'Url', 'Sku', 'TierPrices', 'OldPrice', 'Price',
                                 'ProductSpecificationAttributes'})

    data = []
    for pCard in p:
        print(pCard)
        host = hosts[0] #todo дописать поиск host от vendor
        data = get_pars_data(data, host,  pCard.get('Url'), isPicture = False, isTechs = False, isName=False,
                             isBrand=False, isCategory=False)
        for d in data:
            if d.get('sku') == pCard.get('Sku'):
                print(d)
                update_price_in_product_card(db, d, pCard)

    check_main_product_without_price(db)

def get_colorbase(db, nameIn):
    colors = []

    if os.path.exists(jsonFileNameOfColorBase):
        with open(jsonFileNameOfColorBase, encoding='utf-8') as json_file:
            colors = json.load(json_file)

    if len(colors) == 0:
        sa = find_document(db.SpecificationAttribute, {'Name': 'colorbase'})
        colors = sa.get('SpecificationAttributeOptions')

    retColor = None
    for i, color in enumerate(colors):
        nameColor = color.get('Name')

        if nameColor.lower().strip().split(';').__contains__(nameIn.strip().lower().strip()):
            retColor = colors[i]
            break

    return retColor

def option_sp_color_update_ColorSquaresRgb(db):
    sa = find_document(db.SpecificationAttribute, {'Name': 'sp_color'})
    options = sa.get('SpecificationAttributeOptions')
    for i, option in enumerate(options):
        colorbase = get_colorbase(db, option.get('Name'))

        if not colorbase is None:
            options[i]['ColorSquaresRgb'] = colorbase.get('ColorSquaresRgb')

    update_document(db.SpecificationAttribute, {'_id': sa['_id']}, {'SpecificationAttributeOptions': options})

def option_update_sp_parentId(db):
    for sp_name in _filteringAtribute:
        sa = find_document(db.SpecificationAttribute, {'Name': sp_name})
        options = sa.get('SpecificationAttributeOptions')
        for i, option in enumerate(options):
            if option.get('ParentSpecificationAttrOptionId') == '':
                options[i]['ParentSpecificationAttrOptionId'] = None

        update_document(db.SpecificationAttribute, {'_id': sa['_id']}, {'SpecificationAttributeOptions': options})

def option_color_base_update_name(db):
    sa = find_document(db.SpecificationAttribute, {'Name': 'colorbase'})
    options = sa.get('SpecificationAttributeOptions')
    for i, option in enumerate(options):
        nameOption = option.get('Name')
        if nameOption.find('(HTML: ')>=0:
            nameOption = nameOption.replace('(HTML: ', ';')
            nameOption = nameOption.replace(')', '')

        options[i]['Name'] = nameOption

    update_document(db.SpecificationAttribute, {'_id': sa['_id']}, {'SpecificationAttributeOptions': options})

def pars_option_color_base(db):
    colors = []

    if os.path.exists(jsonFileNameOfColorBase):
        with open(jsonFileNameOfColorBase, encoding='utf-8') as json_file:
            colors = json.load(json_file)

    if len(colors) == 0:
        fmt = 'https://hysy.org/colors/*/1/6532/1'

        soup = get_soup(fmt)
        if soup is None:
            return

        i = 0
        for tag in soup.find_all('div', class_='table-row'):
            hexcode = tag.find('div', class_='table-col h_code').get_text(strip=True)
            colorname = tag.find('div', class_='table-col h_name').get_text(strip=True)
            color = {}
            colorname = colorname.replace('(HTML: ', ';')
            color[hexcode] = colorname
            colors.append(color)
            i = i + 1
            print('{0}. {1}'.format(i, color))
        dump_to_json(jsonFileNameOfColorBase, colors)

    prop_name = 'colorbase'
    for color in colors:
        for color_hex, color_name in color.items():
            if color_name.find('(HTML: ') >= 0:
                color_name = color_name.replace('(HTML: ', ';')
                color_name = color_name.replace(')', '')
            check_specificationattributeoption_by_name(db, prop_name, color_name, color_hex = color_hex)

def pars_new_card_into_bikelookup(db, vendor):

    for host in hosts:
        if host.get('VENDOR') != vendor:
            continue

        jsonfilename = '{0}_{1}_{2}.json'.format(host['OUT_FILENAME'], PAGES_START, (PAGES_START + host['PAGES_COUNT']))
        data = []
        if os.path.exists(jsonfilename):
            with open(jsonfilename, encoding='utf-8') as json_file:
                data = json.load(json_file)

        if len(data) == 0:
            urls = crawl_products(host)

            try:
                data = parse_products(urls, host, isPicture=host.get('isPicture'), isTechs=host.get('isTechs'), cntPars=0)
            except ImportError:
                dump_to_json(jsonfilename, data)
                return

            dump_to_json(jsonfilename, data)

        check_product(db, data)

def resave_option_color_base():
    colors = []

    if os.path.exists(jsonFileNameOfColorBase):
        with open(jsonFileNameOfColorBase, encoding='utf-8') as json_file:
            colors = json.load(json_file)

    colors_new = []

    for color in colors:
        for color_hex, color_name in color.items():
            color_new = {
                'Name': color_name,
                'ColorSquaresRgb': color_hex
            }
            colors_new.append(color_new)

    dump_to_json(jsonFileNameOfColorBase, colors_new)

def clear_all_product(db):
    products = find_document(db.Product, {}, multiple = True)
    for product in products:
        pictures = product.get('ProductPictures')
        SeName = product.get('SeName')

        for picture in pictures:
            delete_document(db.Picture, {'_id': picture.get('PictureId')})

        delete_document(db.Product, {'_id': product.get('_id')})
        delete_document(db.UrlRecord, {'EntityName': 'Product', 'Slug': SeName})

    ads = find_document(db.Ad, {}, multiple = True)
    for ad in ads:
        delete_document(db.Ad, {'_id': ad.get('_id')})

    PrivateMessages = find_document(db.PrivateMessage, {}, multiple = True)
    for pm in PrivateMessages:
        delete_document(db.PrivateMessage, {'_id': pm.get('_id')})

def main():

    client = MongoClient(NAMEMACHINE, PORTDB)
    db = client[NAMEDB]
    #resave_option_color_base()

    #check_actual_price_and_available(db)

    #pars_new_card_into_bikelookup(db, 'bike-family.com.ua')
    #pars_new_card_into_bikelookup(db, 'velogo.com.ua')
    #pars_option_color_base(db)
    #option_color_base_update_name(db)
    #option_sp_color_update_ColorSquaresRgb(db)
    #option_update_sp_parentId(db)
    clear_all_product(db)
if __name__ == '__main__':
    main()
