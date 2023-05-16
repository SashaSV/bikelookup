from database import Database
from schema import StoreSchema

store_schema = StoreSchema()

Database.initialize()

user_input = input("Enter a store dictionary: ")
user_dict = json.loads(user_input)
validated_dict = store_schema.load(user_dict)

Database.save_to_db(validated_dict)

loaded_objects = Database.load_from_db({"name": "Walmart"})

for loaded_store in loaded_objects:
    store = Store(**store_schema.load(loaded_store))