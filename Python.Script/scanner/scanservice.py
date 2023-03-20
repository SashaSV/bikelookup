from bs4 import BeautifulSoup
import json
import os
import posixpath
import requests
from urllib.parse import urlsplit, unquote

HEADERS = {'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36',
           'accept': '*/*'}

def get_soup(url, **kwargs):
    response = requests.get(url, headers=HEADERS, **kwargs)
    if response.status_code == 200:
        soup = BeautifulSoup(response.text, features='html.parser')
    else:
        soup = None
    return soup

def url2filename(url):
    urlpath = urlsplit(url).path
    basename = posixpath.basename(unquote(urlpath))
    if (os.path.basename(basename) != basename or
        unquote(posixpath.basename(urlpath)) != basename):
        raise ValueError  # reject '%2f' or 'dir%5Cbasename.ext' on Windows
    return basename

CURR_DIR = os.getcwd()
if CURR_DIR.find("Python.Script") < 0 :
    CURR_DIR = '{0}\\Python.Script'.format(CURR_DIR)
    
def getDataFromJsonFile(jsonfilename):
    data = []
    
    jsonfilename = '{0}\\{1}'.format(CURR_DIR,jsonfilename)

    if os.path.exists(jsonfilename):
        with open(jsonfilename, encoding='utf-8') as json_file:
            data = json.load(json_file)
    return data

from urllib.request import urlretrieve

def load_image(url,filename):
    if not os.path.exists(filename):
        urlretrieve(url, filename)
        #urlretrieve(url, './')

def dump_to_json(filename, data, **kwargs):
    kwargs.setdefault('ensure_ascii', False)
    kwargs.setdefault('indent', 1)
    
    filename = '{0}\\{1}'.format(CURR_DIR,filename)
    
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(data, f, **kwargs)

def dumps_to_json(filename, data, clsEncode):
    #print(json.dumps(data, cls=clsEncode))
    filename = '{0}\\{1}'.format(CURR_DIR,filename)
    
    with open(filename, 'w', encoding='utf-8') as f:
        json.dump(data, f, cls=clsEncode)

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

def price_without_space(price_with_spac):
    
    price_with_spac = price_with_spac.replace('€','')
    price_with_spac = price_with_spac.replace('.','')
    price_with_spac = price_with_spac.replace(',','.')

    pp = price_with_spac.split()
    
    price_without_space = ''.join(pp)

    return price_without_space
