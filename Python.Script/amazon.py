# -*- coding: utf-8 -*-
from scanner import dbservice, scanservice
from scanner.gethtml import get_html
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
    'url': 'https://www.amazon.es/s?i=computers&bbn=938008031&rh=n%3A938008031%2Cp_n_feature_twenty-two_browse-bin%3A27387615031%2Cp_89%3AApple&dc&page={page}&qid=1681645819&rnid=1692911031&ref=sr_pg_{page}'
}

client = MongoClient(DBCONNECT['NAMEMACHINE'], DBCONNECT['PORTDB'])
#client = MongoClient('mongodb+srv://admin:Zazimja129shura@cluster0.ofiehaa.mongodb.net/bldb')
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

        if len(dataJson) > 0:
            scanservice.dump_to_json(jsonfilename, dataJson)
    
    if len(data) > 0:
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
        
        find_result = soup.find('span', class_='rush-component s-latency-cf-section')
        if find_result is None:
            break

        html_urls = find_result.find_all('div', class_='sg-col-inner')
        
        if html_urls is None:
            break
        
        for tag in html_urls:

            href = tag.find('a').get('href')
            
            if href.find('/ref=') > 0 :
                #href = href[0:href.find('/ref=')].strip()
            
                url = '{0}{1}'.format(HOST, href)

                if url.strip() == (HOST.strip() + '/') :
                    continue

                print(url)
                urls.append(url)

    return urls

def check_property(techs, text):
    ret = ''
    if not techs.get(text) is None:
        ret = techs.get(text)
    return ret

def parse_products(urls) -> list[DataScraps]:
    """
    Парсинг полей:
        название, цена и таблица характеристик
    по каждому товару.
    :param urls:            список URL на карточки товаров.
    :return:                массив спарсенных данных по каждому из товаров.
    """
    seleniumScrapUrl = []
    data = []

    for number, url in enumerate(urls, start=1):
        print('#{num}, product: {url}'.format(num=number, url=url))

        scrapsData = DataScraps(vendor=VENDOR)
        #soup = scanservice.get_soup(url)
        soup = get_html(url)
        if url.find('/ref='):
            scrapsData.url = url[0:url.find('/ref=')].strip()
        
        scrapsData.sku = scrapsData.url[scrapsData.url.rfind('/')+1:]
        
        if soup is None:
            seleniumScrapUrl.append(url)
            break
        try:
            # name
            name = soup.find('span', class_='a-size-large product-title-word-break')
            
            if name is None:
                seleniumScrapUrl.append(url)
                continue
            
            scrapsData.name = name.get_text(strip=True)
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

            scrapsData.display = check_property(techs, 'Tamaño de pantalla')
            scrapsData.hdd = check_property(techs, 'Tamaño del disco duro')
            scrapsData.memory = check_property(techs, 'Tamaño de memoria RAM instalada')
            scrapsData.color = check_property(techs, 'Color')

            scrapsData.techs = dict2obj(techs)
            if soup.find('input', id='attach-baseAsin') is None:
                sku = scrapsData.sku
            else:
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

            for image in images:
                i = i + 1
                urlimage = image.get('hiRes')
                if not urlimage is None:
                    scrapsData.images.append(urlimage)

            if len(scrapsData.images) == 0:
                for script in soup.find_all('script', type="text/javascript"):
                    script = script.get_text(strip=True)
                    if script.find('ImageBlockATF') > 0:
                        script = script.replace("'",'"')
                        
                        jstext = script[script.find('"colorImages"'): script.find('"colorToAsin"')].strip()
                        jstext = '{'+jstext[0:len(jstext)-1]+'}'
                        
                        d = json.loads(jstext)

                        for item in d['colorImages'].get('initial'):
                            urlimage = item.get('hiRes')
                            
                            if urlimage is None:
                                urlimage = item.get('large')
                            
                            if not urlimage is None:
                                scrapsData.images.append(urlimage)
                        continue

            for script in soup.find_all('script', type="text/javascript"):
                script = script.get_text(strip=True)

                if script.find('twister-js-init-dpx-data') > 0:
                    start = script.find('var dataToReturn = {')+len('var dataToReturn = {')-1
                    finish = script.find('return dataToReturn;')    
                    jstext = script[start : finish].strip()
                    jstext = jstext[0:len(jstext)-1]
                    
                    jstext = jstext.replace('\n','')
                    jstext = jstext.replace('\\','')
                    jstext = jstext.replace(' ','')
                    jstext = '['+jstext+']'
                    scanservice.dump_to_json('test.json', jstext)
                    #d = json.loads(jstext)

            if len(images) == 0:
                seleniumScrapUrl.append(url)

            price = '0'
            oldprice = '0'
            
            scrapsData.price = 0.0
            scrapsData.oldprice = 0.0

            prices = soup.find('div', id='corePriceDisplay_desktop_feature_div')

            if prices is None:
                prices = soup.find('span', class_='a-price a-text-price a-size-medium apexPriceToPay')
                
            if prices is None:
                seleniumScrapUrl.append(url)
                continue

            price = prices.find('span', class_='a-price aok-align-center reinventPricePriceToPayMargin priceToPay')

            if not price is None:
                price = price.find('span', class_='a-offscreen').get_text(strip=True)

            if price is None:
                price = prices.find('span', class_='a-offscreen').get_text(strip=True)
                
            if price is None:
                seleniumScrapUrl.append(url)
                continue

            scrapsData.price = float(scanservice.price_without_space(price))

            oldprice = prices.find('div', class_='a-section a-spacing-small aok-align-center')
            
            if not oldprice is None:
                oldprice = oldprice.find('span', class_='a-offscreen')
                
                if not oldprice is None:

                    oldprice = oldprice.get_text(strip=True)
                    scrapsData.oldprice = float(scanservice.price_without_space(oldprice))

            scrapsData.available = 'Vendido'
            av = soup.find('span', class_='a-size-medium a-color-success')
            if not av is None:
                scrapsData.available = av.get_text(strip=True)

            if price.isdigit() and float(price) > 0 and scrapsData.available == 'En stock':
                scrapsData.available = 'En stock'

            scrapsData = pars_name(db,scrapsData)
            data.append(scrapsData)

        except ImportError:
            seleniumScrapUrl.append(url)
            continue

    print(seleniumScrapUrl)
    return data

from datetime import datetime
def pars_name(db, dataScraps: DataScraps) -> DataScraps:
    deletewords = []

    name = dataScraps.name

    for word in deletewords:
        name = name.replace(word, '').strip()

    for n in name.split(' '):
        manufacturer = dbservice.chek_so_name(db, n, 'manufacturer')
        if len(manufacturer) > 0:
            dataScraps.manufacturer = manufacturer
            
        year = dbservice.chek_so_name(db, n, 'year')
        
        if len(year) == 0:
            curyear = datetime.now().year+1
            y = curyear

            while y >= curyear - 15:
                if name.find(str(y)) > 0:
                    year = str(y)
                    

                y = y - 1

        if len(year) > 0:
            dataScraps.year = year
        
        cpu = dbservice.chek_so_name(db, n, 'cpu')
        if len(cpu) > 0:
            dataScraps.cpu = cpu
    
    display = dbservice.chek_so_name(db, name, 'display')
    if len(display) > 0:
        dataScraps.display = display

    memory = dbservice.chek_so_name(db, name, 'memory')
    if len(memory) > 0:
        dataScraps.memory = memory

    hdd = dbservice.chek_so_name(db, name, 'hdd')
    if len(hdd) > 0:
        dataScraps.memory = hdd

    color = dbservice.chek_so_name(db, name, 'color')
    if len(color) > 0:
        dataScraps.color = color

    dataScraps.model = find_model(name)

    return dataScraps

import sys
sys.setrecursionlimit(1500)

def find_model(s:str, retModels:list[str] = [])->list[str]:
    if len(s) > 0:
        for n in s.split(' '):
            s = s.replace(n,'').strip()
            model = dbservice.chek_so_name(db, n, 'model')
            if len(model) > 0:
                retModels.append(model)
                find_model(s)
            if len(retModels) > 0:
                break
    
    return retModels

def main():
    pars_new_card_into_db()

if __name__ == '__main__':
    main()
