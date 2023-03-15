# -*- coding: utf-8 -*-
from scanner import dbservice, scanservice
from scanner.dbservice import DataScraps, dict2obj
from pymongo import MongoClient
import json
import copy

#from munch import DefaultMunch

# 86
DBCONNECT = {'NAMEMACHINE': 'localhost',
             'PORTDB': 27017,
             'NAMEDB': 'bldb'}

PAGES_START = 1
PAGES_COUNT = 2
OUT_FILENAME = 'amazon'
OUT_XLSX_FILENAME = 'amazon'
VENDOR = 'amazon.es'
HOST = 'https://www.amazon.es'
OUT_FILE_CATALOG = '/photo/'
#FTM = 'https://www.amazon.es/s?k=apple&i=computers&page={page}&crid=32CAS3PB7DQ6I&qid=1678216929&sprefix=ap%2Ccomputers%2C181&ref=sr_pg_{page}'

FTM = {
    'category': 'Portátiles',
    'url': 'https://www.amazon.es/s?k=Apple&i=computers&rh=n%3A2457640031%2Cp_4%3AApple&page={page}&pf_rd_i=938008031&pf_rd_m=A1AT7YVPFBWXBL&pf_rd_p=bc93f755-eff3-453b-9e77-c2f6b3d2f854&pf_rd_p=bc93f755-eff3-453b-9e77-c2f6b3d2f854&pf_rd_r=E5N2A2BM8CZY8ZC7PHA5&pf_rd_r=E5N2A2BM8CZY8ZC7PHA5&pf_rd_s=merchandised-search-leftnav&pf_rd_t=101&qid=1678560440&ref=sr_pg_{page}'
}

client = MongoClient(DBCONNECT['NAMEMACHINE'], DBCONNECT['PORTDB'])
db = client[DBCONNECT['NAMEDB']]

def pars_new_card_into_db():
        
    jsonfilename = '{0}_{1}_{2}.json'.format(OUT_FILENAME, PAGES_START, (PAGES_START + PAGES_COUNT))
    jsonUrlFileName = 'URL_{0}_{1}_{2}.json'.format(OUT_FILENAME, PAGES_START, (PAGES_START + PAGES_COUNT))
    data = scanservice.getDataFromJsonFile(jsonfilename)
    
    #d1 = DefaultMunch.fromDict(data, DataScraps())
    data = dict2obj(data)
        
    if len(data) == 0:
        urls = scanservice.getDataFromJsonFile(jsonUrlFileName)
        
        if len(urls) == 0:
            urls = crawl_products(PAGES_COUNT)
            scanservice.dump_to_json(jsonUrlFileName, urls)
        
        data = parse_products(urls)
        dataJson = []
        for row, item in enumerate(data, start=0):
            a = copy.copy(item)
            a.techs = item.techs.__dict__
            dataJson.append(a.__dict__)

        scanservice.dump_to_json(jsonfilename, dataJson)
    
    dbservice.clear_all_product(db)
    dbservice.check_product(db, data)

def crawl_products(pages_count):
    """
    Собирает со страниц с 1 по pages_count включительно ссылки на товары.
    :param pages_count:     номер последней страницы с товарами.
    :return:                список URL товаров.
    """
    urls = []
    
    for page_n in range(PAGES_START, PAGES_START + pages_count):
        print('page: {0}'.format(page_n))

        page_num = 1 if page_n == 1 else page_n

        page_url = FTM['url'].format(page=page_num)
        soup = scanservice.get_soup(page_url)

        if soup is None:
            break

        for tag in soup.find_all('div', class_='a-section a-spacing-base'):

            href = tag.find('a').get('href')
            
            #if href.find('/ref='):
            #    href = href[0:href.find('/ref=')].strip()
            
            url = '{0}{1}'.format(HOST, href)

            if url.strip() == (HOST.strip() + '/') :
                continue

            print(url)
            urls.append(url)

    return urls

def parse_products(urls) -> list[DataScraps]:
    """
    Парсинг полей:
        название, цена и таблица характеристик
    по каждому товару.
    :param urls:            список URL на карточки товаров.
    :return:                массив спарсенных данных по каждому из товаров.
    """

    data = []

    for number, url in enumerate(urls, start=1):
        print('#{num}, product: {url}'.format(num=number, url=url))

        scrapsData = DataScraps(vendor=VENDOR)
        soup = scanservice.get_soup(url)
        
        if url.find('/ref='):
            scrapsData.url = url[0:url.find('/ref=')].strip()
        
        scrapsData.sku = scrapsData.url[scrapsData.url.rfind('/'):]
        
        if soup is None:
            break
        try:
            # name
            scrapsData.name = soup.find('span', class_='a-size-large product-title-word-break').get_text(strip=True)

            # brand
            scrapsData.manufacturer = 'Apple'

            # category
            scrapsData.category = FTM['category']

            # options
            techs = {}
            for row in soup.find_all('tr', class_='a-spacing-small'):
                nameoption = row.find('td', class_='a-span3').get_text(strip=True)
                valueoption = row.find('td', class_='a-span9').get_text(strip=True)
                techs[nameoption] = valueoption
            
            scrapsData.techs = dict2obj(techs)
            if soup.find('input', id='attach-baseAsin') is None:
                continue
                
            sku = soup.find('input', id='attach-baseAsin').get('value')
            scrapsData.sku = sku
            # images
            images = []
            i = 0
            
            script = soup.find('div', id='imageBlockVariations_feature_div').find('script').get_text(strip=True)
            jstext = script[script.find('jQuery.parseJSON(')+len('jQuery.parseJSON(')+1 : script.find('''}\')''',script.find('jQuery.parseJSON('))+1]
            jstext = '['+jstext+']'
            d = json.loads(jstext)
            
            images = []
            for row, item in enumerate(d[0].get('colorToAsin'), start=0):
                if d[0].get('colorToAsin')[item].get('asin') == sku:
                    images = d[0].get('colorImages').get(item)
                    continue
            
            if len(images) == 0:
                imgs = soup.find('ul', class_='a-unordered-list a-nostyle a-button-list a-vertical a-spacing-top-extra-large regularAltImageViewLayout').find_all('li')
                for img in imgs:
                    i = img.find('img').get('src')
                    #i = i[0:]
                    scrapsData.images.append()

            for image in images:
                i = i + 1
                urlimage = image.get('hiRes')
                scrapsData.images.append(urlimage)

            prices = soup.find('div', id='corePriceDisplay_desktop_feature_div')

            if prices is None:
                continue

            price = '0'
            oldprice = '0'

            price = prices.find('span', class_='a-price aok-align-center reinventPricePriceToPayMargin priceToPay').find('span', class_='a-offscreen').get_text(strip=True)

            scrapsData.price = float(scanservice.price_without_space(price))

            oldprice = prices.find('div', class_='a-section a-spacing-small aok-align-center')
            if not oldprice is None:
                oldprice = oldprice.find('span', class_='a-offscreen').get_text(strip=True)
                scrapsData.oldprice = float(scanservice.price_without_space(oldprice))

            scrapsData.available = 'Vendido'
            av = soup.find('span', class_='a-size-medium a-color-success')
            if not av is None:
                scrapsData.available = av.get_text(strip=True)

            if price.isdigit() and float(price) > 0 and scrapsData.available == 'En stock':
                scrapsData.available = 'En stock'

            data.append(scrapsData)

        except ImportError:
            continue
    return data

def main():
    pars_new_card_into_db()


if __name__ == '__main__':
    main()
