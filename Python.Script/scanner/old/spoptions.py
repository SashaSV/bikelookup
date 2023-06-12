from database import Database
from storeschema import Category, CategorySchema, dump_to_json
#from modeldb import Category

category_schema = CategorySchema()

Database.initialize("mongodb://localhost:27017/bldb")
Database.database.Ca
loaded_categorys = Database.load_from_db({})

for loaded_category in loaded_categorys:
    loaded_category['CreatedOnUtc'] = loaded_category['CreatedOnUtc'].isoformat()
    loaded_category['UpdatedOnUtc'] = loaded_category['UpdatedOnUtc'].isoformat()
    category = Category(**category_schema.load(loaded_category).__dict__)

    


