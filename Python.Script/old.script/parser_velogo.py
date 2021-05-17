# -*- coding: utf-8 -*-
import json
import urllib
import os
import posixpath
import xlsxwriter
import requests
from bs4 import BeautifulSoup

#86
PAGES_START = 1
PAGES_COUNT = 1
OUT_FILENAME = 'velogo'
OUT_XLSX_FILENAME = 'velogo'
VENDOR = 'velogo.com.ua'
HOST = 'https://velogo.com.ua'
OUT_FILE_CATALOG = 'F://PythonProj/upload/velo/'

SPECNAMEBASE = {
    #general/общая
    'sp_year': 'Модельный год',
    'sp_typebike': 'Тип велосипеда',
    'sp_for': 'Для кого',
    'sp_age': 'Возраст (Рост)',
    'sp_height': 'Рост',
    'sp_size': 'Размер рамы',

    #transmission/трансмиссия
    'sp_equipment': 'Оборудование',
    'sp_count_speed': 'Количество скоростей',
    'sp_shift': 'Ручки переключения (манетки, дуалы)',
    'sp_front_derailleur': 'Передний переключатель',
    'sp_rear_derailleur': 'Задний переключатель',
    'sp_crankset': 'Система (шатуны)',
    'sp_cassette': 'Задние звезды',
    'sp_chain': 'Цепь',

    #frameset/рама
    'sp_material_frame': 'Материал рамы',
    'sp_type_fork': 'Тип вилки',
    'sp_brand_fork': 'Бренд вилки',
    'sp_frame': 'Рама',
    'sp_fork': 'Вилка',
    'sp_Fork_travel': 'Ход вилки',
    'sp_seatpost': 'Подседельная труба (глагол)',
    'sp_headset': 'Рулевая колонка',
    'sp_carriage': 'Каретка',
    'sp_rear_shock_absorber': 'Задний амортизатор',

    #brakes/тормоза
    'sp_type_brake': 'Тип тормозов',
    'sp_front_brake': 'Передний тормоз',
    'sp_rear_brake': 'Задний тормоз',
    'sp_front_rotor': 'Ротор передний',
    'sp_rear_rotor': 'Ротор задний',
    'sp_brakes': 'Тормоза',
    'sp_brake_levers': 'Тормозные ручки',

    #wheels/колеса
    'sp_wheeldiams': 'Размер колес',
    'sp_front_hub': 'Передняя втулка',
    'sp_type_rear_hub': 'Тип задней втулки',
    'sp_rear_hub': 'Задняя втулка',
    'sp_rims': 'Обод',
    'sp_weels': 'Колеса',
    'sp_hubs': 'Втулки',
    'sp_tyres': 'Покрышки',
    'sp_front_tyre': 'Покрышка передняя',
    'sp_rear_tyre': 'Покрышка задняя',
    'sp_spokes': 'Спицы',

    #other/прочее
    'sp_handlebars': 'Руль',
    'sp_stem': 'Вынос',
    'sp_weight_Limit': 'Вес велосипедиста, кг',
    'sp_weight_bike': 'Вес',
    'sp_grips': 'Ручки руля (грипсы, обмотка)',
    'sp_sedals': 'Педали',
    'sp_saddle': 'Седло',
    'sp_other': 'Дополнительно',

    #e-motor/е-мотор
    'sp_Motor': 'Мотор',
    'sp_battery': 'Батарея',
    'sp_charger': 'Зарядное',
    'sp_display': 'Дисплей'
}

def get_soup(url, **kwargs):
    response = requests.get(url, **kwargs)
    if response.status_code == 200:
        soup = BeautifulSoup(response.text, features='html.parser')
    else:
        soup = None
    return soup

def get_headers_spec(spec_value):

    ret_name_spec = spec_value
    for sp_ind, sp_name in enumerate(SPECNAMEBASE):
        #print(sp_ind, sp_name, SPECNAMEBASE[sp_name])
        if (spec_value == SPECNAMEBASE[sp_name]):
            ret_name_spec = sp_name
            break

    return ret_name_spec

def crawl_products(pages_count):
    """
    Собирает со страниц с 1 по pages_count включительно ссылки на товары.
    :param pages_count:     номер последней страницы с товарами.
    :return:                список URL товаров.
    """
    urls = []
    fmt = 'https://velogo.com.ua/velo?per_page={page}'

    for page_n in range(PAGES_START, PAGES_START + pages_count):
        print('page: {}'.format(page_n * 24))

        page_num = 1 if page_n == 1 else page_n * 24

        page_url = fmt.format(page=page_num)
        soup = get_soup(page_url)
        if soup is None:
            break

        i = 0
        for tag in soup.select('.product-cut__title-link'):
            available = tag.find('span', class_='product-photo__not_available product-photo__not_available--size_c hidden')
            if available is None:
               continue

            href = tag.attrs['href']
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
    retsize = ''

    if sizevalue.strip() != '':
        newsize = sizevalue.strip().replace(' ', '/', 1).split('/')
        if len(newsize) > 1:
            if len(newsize[0]) < 7 and len(newsize[1]) > 0:
                if newsize[1][0] != '(':
                    newsize[1] = '(' + newsize[1] + ')'

            retsize = newsize[0]

            #+ '/' + newsize[1]

    return retsize

def appintodata(data,url,name,sku,brand,category,price,oldprice,VENDOR,size,available,techs,images):

    #
    deletewords = ['Рама ', 'рама ', 'зріст', 'Размер рамы ', 'на рост ', 'рост ', '(колеса 27,5")', 'колеса 27,5', 'колеса 27.5']

    for word in deletewords:
        size = size.replace(word, '')

    size = sizemodify(size)

    item = {
        'url': url,
        'name': name,
        'sku': sku,
        'Manufacturer': brand,
        'Category': category,
        'price': price,
        'oldprice': oldprice,
        'Vendor': VENDOR,
        'sp_size': size,
        'sp_available': available,
        'techs': techs,
        'picture': images
    }

    data.append(item)

def parse_products(urls, withloadimage, picture = False, attr = False):
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
        print('#{num}, product: {url}'.format(num=number, url=url))

        soup = get_soup(url)
        if soup is None:
            break

        try:
            #name
            name = soup.find('h1', class_='content__title').get_text(strip=True)
            name = name.replace('УЦЕНКА -', '')
            #brand
            brandobj = soup.find('img', class_='brands-widget__item')

            if brandobj is None:
                brandobj = soup.find('span', class_='brands-widget__item')
                brand = brandobj.get_text(strip=True)
            else:
                brand = brandobj.get('alt')
                brandimg = HOST + brandobj.get('src')

            #category
            categoreis = soup.find_all('li', class_='breadcrumbs__item')

            category = categoreis[3].get_text(strip=True)

            if (len(categoreis) < 5):
                category = categoreis[2].get_text(strip=True)

            category = 'Велосипеды'

            # options
            techs = {}
            if attr:
                for row in soup.find_all('div', class_='properties__item'):
                    nameoption = row.find('span', class_='tooltip__label').get_text(strip=True)
                    valueoption = row.find('div', class_='properties__value').get_text(strip=True)
                    techs[get_headers_spec(nameoption)] = valueoption

            #images
            images = {}
            if picture:
                i=0
                for image in soup.find_all('a', class_='product-photo__thumb-item'):
                    i = i+1
                    urlimage = image.get('href')
                    filename = OUT_FILE_CATALOG + url2filename(urlimage)
                    images['picture' + str(i)] = filename

                    if withloadimage:
                        load_image(urlimage, filename)


            skubase = soup.find('div', class_='product-intro__addition-item')

            sku = ''
            if not skubase is None:
                sku = skubase.find('span').get_text(strip=True)

            baseprice = soup.find('div', class_='product-intro__price row')
            price = oldprice = '0'

            op = p = None

            if not baseprice is None:
                op = baseprice.find('span', class_='product-price__old-value')
                p = baseprice.find('span', class_='product-price__main-value')

            if not op is None:
                oldprice = op.get_text(strip=True)

            if not p is None:
                price = p.get_text(strip=True)

            #price, size, available
            aa = soup.find_all('div', class_='variants-radio__item')

            if len(aa) == 0:
                continue
                available = 'Продано'
                size = ''

                oldprice = price_without_space(oldprice)
                price = price_without_space(price)

                if int(price) > 0:
                    available = 'В наличии'
                else:
                    continue

                appintodata(data, url, name, sku, brand, category, price, oldprice,
                            VENDOR, size, available, techs, images)

            # print(data)
            for a1 in aa:
                available = a1.find('span', class_='variants-radio__available')
                if available is None:
                    available = a1.find('a', class_='variants-radio__available')

                if available is None:
                    continue
                available = available.get_text(strip=True)

                if available == 'Продано':
                    continue

                variants = a1.find('span', class_='variants-radio__field-inner')

                sizes = variants.find_all('span', class_='variants-radio__title')
                size = sizes[1].get_text(strip=True)
                new_techs = {}
                if attr:
                    diams = techs['sp_wheeldiams'].split(', ')
                    new_techs = techs.copy()
                    new_techs['sp_wheeldiams'] = diams[0]
                    if (size.find('27,5') > 0 or size.find('27.5') > 0):
                        if (len(diams) > 1):
                            new_techs['sp_wheeldiams'] = diams[1]
                        else:
                            new_techs['sp_wheeldiams'] = '27,5'

                cardoption = variants.find('input', class_='delivery-radio__check')
                sku = cardoption.get('data-product-variant--number').strip()

                price = oldprice = '0'

                op = a1.find('span', class_='product-price__old-value')

                if not op is None:
                    oldprice = op.get_text(strip=True)

                p = a1.find('span', class_='product-price__main-value')

                if not p is None:
                    price = p.get_text(strip=True)

                oldprice = price_without_space(oldprice)
                price = price_without_space(price)
                appintodata(data, url, name, sku, brand, category, price, oldprice, VENDOR, size, available, new_techs, images)
                # print(data)

        except ImportError:
            continue

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

        headers = ['name', 'url', 'sku', 'Manufacturer', 'Category', 'price', 'oldprice', 'Vendor', 'sp_size', 'sp_available']

        uniq_el = all_uniq_name(data, 'techs')
        headers.extend(uniq_el)

        headers.extend(data[max_elem(data,'picture')]['picture'].keys())

        for col, h in enumerate(headers):
            ws.write_string(0, col, h, cell_format=bold)

        for row, item in enumerate(data, start=1):
            ws.write_string(row, 0, item['name'])
            ws.write_string(row, 1, item['url'])
            ws.write_string(row, 2, item['sku'])
            ws.write_string(row, 3, item['Manufacturer'])
            ws.write_string(row, 4, item['Category'])
            ws.write_string(row, 5, item['price'])
            ws.write_string(row, 6, item['oldprice'])
            ws.write_string(row, 7, item['Vendor'])
            ws.write_string(row, 8, item['sp_size'])
            ws.write_string(row, 9, item['sp_available'])

            for prop_name, prop_value in item['techs'].items():
                col = headers.index(prop_name)
                ws.write_string(row, col, prop_value)

            for prop_name, prop_value in item['picture'].items():
                col = headers.index(prop_name)
                ws.write_string(row, col, prop_value)

def main():
    urls = crawl_products(PAGES_COUNT)
    data = parse_products(urls, True, picture = True, attr = False)
    #dump_to_json('{0}_{1}_{2}.json'.format(OUT_FILENAME, PAGES_START, (PAGES_START+PAGES_COUNT)), data)
    dump_to_xlsx('{0}_{1}_{2}.xlsx'.format(OUT_XLSX_FILENAME, PAGES_START, (PAGES_START+PAGES_COUNT)), data)

if __name__ == '__main__':
    main()
