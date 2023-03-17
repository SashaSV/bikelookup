import requests
from bs4 import BeautifulSoup
from selenium import webdriver
import time
import os
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.support.wait import WebDriverWait
from selenium.webdriver.support import expected_conditions
from selenium.webdriver.common.by import By
from selenium.common.exceptions import TimeoutException
import json

def get_html(url) -> None:

    paswd = "/chromedriver"

    #s = Service(paswd)
    s = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=s)

    #driver = webdriver.Chrome(
    #    executable_path=paswd
    #)

    #driver.get("https://www.google.com")
    #driver = webdriver.Chrome(service=ser, options=op)

    html_text = None
    file_name = 'source_page.html'
    
    if os.path.exists(file_name):
        with open(file_name, encoding='utf-8') as thtml_file:
            html_text = thtml_file.read()  # if you only wanted to read 512 bytes, do .read(512)
            thtml_file.close()
    
    if html_text is None:
        driver.maximize_window()
        driver.get(url)
        time.sleep(3)
        html_text = driver.page_source
        with open("source_page.html", "w", encoding="utf-8") as file:
            file.write(driver.page_source)
    
    soup = BeautifulSoup(html_text, features='html.parser')
    images = []
    for script in soup.find_all('script', type="text/javascript"):
        script = script.get_text(strip=True)
        if script.find('ImageBlockATF') > 0:
            script = script.replace("'",'"')
            jstext = script[script.find('"colorImages"'): script.find('"colorToAsin"')].strip()
            jstext = '{'+jstext[0:len(jstext)-1]+'}'
            d = json.loads(jstext)

            for item in d['colorImages'].get('initial'):
                images.append(item.get('hiRes'))
            
            continue
    
    try:   
        a=1
    except Exception as _ex:
        print(_ex)

    finally:
        driver.close()
        driver.quit()