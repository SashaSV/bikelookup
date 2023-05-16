import pymongo

class Database:
	
	@classmethod
	def initialize(cls, dbconnect):
		client = pymongo.MongoClient(dbconnect)
		cls.database = client.get_default_database()

	@classmethod
	def insert_to_db(cls, data):
		cls.database.Category.insert_one(data)

	@classmethod
	def load_from_db(cls, query):
		return cls.database.Category.find(query)