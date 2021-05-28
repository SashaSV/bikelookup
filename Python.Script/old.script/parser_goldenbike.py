# -*- coding: utf-8 -*-
import json
import urllib
import os
import posixpath
import xlsxwriter
import requests
from bs4 import BeautifulSoup

# 86
PAGES_START = 1
PAGES_COUNT = 28
OUT_FILENAME = 'goldenbike'
OUT_XLSX_FILENAME = 'goldenbike'
VENDOR = 'goldenbike.com.ua'
HOST = 'https://goldenbike.com.ua'
OUT_FILE_CATALOG = 'F://PythonProj/upload/velo/'
HEADERS = {'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:71.0) Gecko/20100101 Firefox/71.0',
           'accept': '*/*'}

SPECNAMEBASE = {
    # general/общая
    'sp_year': 'Год',
    'sp_typebike': 'Шаблон продукта',
    'sp_for': 'Пол и возраст',
    'sp_age': 'Возраст (Рост)',
    'sp_height': 'Рост',
    'sp_size': '-Размер рамы',
    'sp_available': 'Доступность',
    'sp_model': 'Модель',

    # transmission/трансмиссия
    'sp_equipment': 'оборудование',
    'sp_count_speed': 'Количество скоростей',
    'sp_shift': 'Манетки (ручки переключения)',
    'sp_front_derailleur': 'Передний переключатель',
    'sp_rear_derailleur': 'Задний переключатель',
    'sp_crankset': 'Система',
    'sp_cassette': 'Кассета',
    'sp_chain': 'Цепь',

    # frameset/рама
    'sp_material_frame': 'Материал рамы',
    'sp_type_fork': 'Тип вилки',
    'sp_brand_fork': 'Бренд вилки',
    'sp_frame': 'Рама',
    'sp_fork': 'Вилка',
    'sp_Fork_travel': 'Ход вилки',
    'sp_seatpost': 'Подседельный штырь',
    'sp_headset': 'Рулевая колонка',
    'sp_carriage': 'Каретка',
    'sp_rear_shock_absorber': 'Задний амортизатор',

    # brakes/тормоза
    'sp_type_brake': 'Тип тормозов',
    'sp_front_brake': 'Тормоз передний',
    'sp_rear_brake': 'Тормоз задний',
    'sp_front_rotor': 'Ротор передний',
    'sp_rear_rotor': 'Ротор задний',
    'sp_brakes': 'Тормоза',
    'sp_brake_levers': 'Тормозные ручки',
    # wheels/колеса
    'sp_wheeldiams': 'Диаметр колеса',
    'sp_front_hub': 'Втулка передняя',
    'sp_type_rear_hub': 'Тип задней втулки',
    'sp_rear_hub': 'Втулка задняя',
    'sp_rims': 'Обода',
    'sp_weels': 'Колеса',
    'sp_hubs': 'Втулки',
    'sp_tyres': 'Покрышки',
    'sp_front_tyre': 'Передняя покрышка',
    'sp_rear_tyre': 'Задняя покрышка',
    'sp_spokes': 'Спицы',
    # other/прочее
    'sp_handlebars': 'Руль',
    'sp_stem': 'Вынос',
    'sp_weight_Limit': 'Вес велосипедиста, кг',
    'sp_weight_bike': 'Вес',
    'sp_grips': 'Ручки руля (грипсы)',
    'sp_sedals': 'Педали',
    'sp_saddle': 'Седло',
    'sp_other': 'Примечание',
    # e-motor/е-мотор
    'sp_Motor': 'Мотор',
    'sp_battery': 'Батарея',
    'sp_charger': 'Зарядное',
    'sp_display': 'Дисплей'
}


def get_soup(url, **kwargs):
    response = requests.get(url, headers=HEADERS, **kwargs)
    if response.status_code == 200:
        soup = BeautifulSoup(response.text, features='html.parser')
    else:
        soup = None
    return soup


def get_headers_spec(spec_value):
    ret_name_spec = spec_value
    for sp_ind, sp_name in enumerate(SPECNAMEBASE):
        # print(sp_ind, sp_name, SPECNAMEBASE[sp_name])
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
    fmt = 'https://goldenbike.com.ua/velosipedy?user_per_page=48&per_page={page}'

    for page_n in range(PAGES_START, PAGES_START + pages_count):
        print('page: {0}'.format(page_n))

        page_num = 1 if page_n == 0 else page_n * 48

        page_url = fmt.format(page=page_num)
        soup = get_soup(page_url)

        if soup is None:
            break
        i = 0
        for tag in soup.find_all('div', class_='col-xs-6 col-sm-6 col-md-3 col-lg-3'):

            href = tag.find('a').get('href')
            url = '{1}'.format(HOST, href)

            if url.strip() == (HOST.strip() + '/') :
                continue

            i = i+1
            print('{0}. {1}'.format(i, url))
            urls.append(url)

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

            retsize = newsize[0] + '/' + newsize[1]

    return retsize


def appintodata(data, url, name, sku, brand, category, price, oldprice, VENDOR, size, available, techs, images):
    # deletewords = ['Рама ', 'рама ', 'зріст', 'Размер рамы ', 'на рост ', 'рост ', '(колеса 27,5")', 'колеса 27,5', 'колеса 27.5']

    # for word in deletewords:
    #    size = size.replace(word, '')

    #ize = sizemodify(size)

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


def parse_products(urls, withloadimage):
    """
    Парсинг полей:
        название, цена и таблица характеристик
    по каждому товару.
    :param urls:            список URL на карточки товаров.
    :return:                массив спарсенных данных по каждому из товаров.
    """

    data = []

    #urls = ['https://goldenbike.com.ua/velosiped-merida-one-forty-700-red-red-2020#comments-list']

    for number, url in enumerate(urls, start=1):
        print('#{num}, product: {url}'.format(num=number, url=url))

        soup = get_soup(url)
        if soup is None:
            break

        try:

            # name
            name = soup.find('h1', itemprop='name')
            if name is None:
                continue

            name = name.get_text(strip=True)

            #brand
            brand = soup.find('a', class_='product-intro__addition-link').get_text(strip=True)

            #category
            category = 'Велосипеды'

            #size
            size = ''

            # options
            techs = {}
            table = soup.find('div', class_='properties')
            for row in table.find_all('div', class_='properties__item'):
                nameoption = row.find('span', class_='tooltip__label')
                if nameoption is None:
                    continue
                nameoption = nameoption.get_text(strip=True)

                valueoption = row.find('div', class_='properties__value')
                if valueoption is None:
                    continue

                valueoption = valueoption.get_text(strip=True)

                techs[get_headers_spec(nameoption)] = valueoption

            # images
            images = {}
            """
            i=0
            for image in soup.find_all('a', class_='product-photo__thumb-item'):
                i = i+1
                urlimage = image.get('href')
                filename = OUT_FILE_CATALOG + url2filename(urlimage)
                images['picture' + str(i)] = filename

                if withloadimage:
                    load_image(urlimage, filename)

            """

            paramsku = soup.find_all('div', class_='variants-radio__item')

            for param in paramsku:

                av = param.find('span', class_='stock_jo green')

                if av is None:
                    av = param.find('span', class_='stock_jo red')

                if not av is None:
                    available = av.get_text(strip=True)

                inputparam = param.find('input', class_="variants-radio__control")

                if inputparam is None:
                    continue

                sku = inputparam.get('data-product-variant--number')

                size = param.find('span', class_='variants-radio__title').get_text(strip=True)

                deletewords = ['Рама -']

                for word in deletewords:
                    size = size.replace(word, '')

                price = param.find('div', class_='product-price__main').\
                    find('span', class_='product-price__item-value').get_text(strip=True)

                price = price.replace('грн.', '')
                price = price_without_space(price)

                oldprice = param.find('div', class_='product-price__old')

                if not oldprice is None:
                    oldprice = oldprice.find('span', class_='product-price__item-value').get_text(strip=True)
                    oldprice = oldprice.replace('грн.', '')
                    oldprice = price_without_space(oldprice)
                else:
                    oldprice = price

                appintodata(data, url, name, sku, brand, category, price, oldprice, VENDOR, size, available, techs,
                            images)

            if paramsku is None:
                available = 'Продано'
                av = soup.find('span', class_='stock-high')

                if av is None:
                    continue

                av = av.get_text(strip=True)

                if av != 'In stock':
                    continue

                if av == 'In stock':
                    available = 'В наличии'

                oldprice = soup.find('form', id='cart-form').find('span','compare-at-price nowrap')

                if not oldprice is None:
                    oldprice = oldprice.get_text(strip=True)
                    oldprice = oldprice.replace('грн.', '')
                    oldprice = price_without_space(oldprice)
                else:
                    oldprice = '0'

        except ImportError:
            continue

    return data


try:
    from urlparse import urlsplit
    from urllib import unquote
except ImportError:  # Python 3
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


def load_image(url, filename):
    if not os.path.exists(filename):
        urlretrieve(url, filename)
    # urlretrieve(url, './')


def dump_to_json(filename, data, **kwargs):
    kwargs.setdefault('ensure_ascii', False)
    kwargs.setdefault('indent', 1)

    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(data, f, **kwargs)


def all_uniq_name(data, name_item):
    uniq_n = []
    for row, item in enumerate(data, start=1):
        if len(item) > 0:
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

        headers = ['name', 'url', 'sku', 'Manufacturer', 'Category', 'price', 'oldprice', 'Vendor', 'sp_size',
                   'sp_available']

        uniq_el = all_uniq_name(data, 'techs')
        headers.extend(uniq_el)

        headers.extend(data[max_elem(data, 'picture')]['picture'].keys())

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
    data = parse_products(urls, False)
    # dump_to_json('{0}_{1}_{2}.json'.format(OUT_FILENAME, PAGES_START, (PAGES_START+PAGES_COUNT)), data)
    dump_to_xlsx('{0}_{1}_{2}.xlsx'.format(OUT_XLSX_FILENAME, PAGES_START, (PAGES_START + PAGES_COUNT)), data)


if __name__ == '__main__':
    main()
