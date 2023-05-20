from database import Database
from storeschema import Category, CategorySchema, dump_to_json
#from modeldb import Category

category_schema = CategorySchema()

Database.initialize("mongodb://localhost:27017/bldb")

loaded_categorys = Database.load_from_db({})

for loaded_category in loaded_categorys:
    #js = dump_to_json('testjson.json', loaded_category)
    category = Category(**category_schema.load(loaded_category))


