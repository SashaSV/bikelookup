#import pymongo
import datetime as dt
from pymongo import MongoClient
from umongo import Document, EmbeddedDocument, fields, validate
from umongo.frameworks import PyMongoInstance

class Database:
	@classmethod
	def initialize(cls, dbconnect):
		client = MongoClient(dbconnect)
		cls.database = client.get_default_database()
		return PyMongoInstance(cls.database)
	
	@classmethod
	def insert_to_db(cls, data):
		cls.database.Category.insert_one(data)

	@classmethod
	def load_from_db(cls, query):
		return cls.database.Category.find(query)

instance = Database.initialize("mongodb://localhost:27017/bldb")

@instance.register
class UrlRecord(Document):
	GenericAttributes = fields.ListField(fields.StrField())
	EntityId = fields.StringField()
	EntityName = fields.StringField()
	Slug = fields.StringField()
	IsActive = fields.BooleanField()
	LanguageId = fields.StringField()
    
	class Meta:
		collection_name = "UrlRecord"

@instance.register
class LocaleSchema(EmbeddedDocument):
    _id = fields.StringField()
    LanguageId = fields.StringField()
    LocaleKey = fields.StringField()
    LocaleValue = fields.StringField()

@instance.register
class CategorySpecificationAttributeSchema(EmbeddedDocument):
    _id = fields.StringField()
    GenericAttributes = fields.ListField(fields.StringField())
    CategoryId = fields.StringField()
    AttributeTypeId = fields.IntegerField()
    SpecificationAttributeId = fields.StringField()
    DetailsUrl = fields.StringField()
    SpecificationAttributeOptionId = fields.StringField()
    CustomValue = fields.StringField()
    AllowFiltering = fields.BooleanField()
    ShowOnProductPage = fields.BooleanField()
    DisplayOrder = fields.IntegerField()
    Locales = fields.ListField(fields.EmbeddedField(LocaleSchema))
    AttributeType = fields.IntegerField()

from bson.objectid import ObjectId

@instance.register
class CategoryTemplate(Document):
    GenericAttributes = fields.ListField(fields.StringField())
    Name = fields.StringField(default=None)
    ViewPath = fields.StringField(default=None)
    DisplayOrder = fields.IntegerField(default=0)
    class Meta:
        collection_name = "CategoryTemplate"
	
@instance.register
class Category(Document):
    _id = fields.StringField(default=str(ObjectId()))
    GenericAttributes = fields.ListField(fields.StringField())
    Name = fields.StringField(default=None)
    SeName = fields.StringField(default=None)
    Description = fields.StringField(default=None)
    BottomDescription = fields.StringField(default=None)
    CategoryTemplateId = fields.StringField(default=CategoryTemplate.find_one().id)
    MetaKeywords = fields.StringField(default=None)
    MetaDescription = fields.StringField(default=None)
    MetaTitle = fields.StringField(default=None)
    ParentCategoryId = fields.StringField(default=None)
    PictureId = fields.StringField(default=None)
    PageSize = fields.IntegerField(default=20)
    AllowCustomersToSelectPageSize = fields.BooleanField(default=False)
    PageSizeOptions = fields.StringField(default="6 3 9")
    PriceRanges = fields.NumberField(default=None)
    ShowOnHomePage = fields.BooleanField(default=False)
    FeaturedProductsOnHomaPage = fields.BooleanField(default=False)
    ShowOnSearchBox = fields.BooleanField(default=False)
    SearchBoxDisplayOrder = fields.IntegerField(default=False)
    IncludeInTopMenu = fields.BooleanField(default=False)
    SubjectToAcl = fields.BooleanField(default=False)
    CustomerRoles = fields.ListField(fields.StringField())
    Stores = fields.ListField(fields.StringField())
    LimitedToStores = fields.BooleanField(default=False)
    Published = fields.BooleanField(default=True)
    DisplayOrder = fields.IntegerField(default=0)
    Flag = fields.StringField(default=None)
    FlagStyle = fields.StringField(default=None)
    Icon = fields.StringField(default=None)
    DefaultSort = fields.IntegerField(default=0)
    HideOnCatalog = fields.BooleanField(default=False)
    CreatedOnUtc = fields.DateTimeField(validate=validate.Range(min=dt.datetime.now()))
    UpdatedOnUtc = fields.DateTimeField(validate=validate.Range(min=dt.datetime.now()))
    AppliedDiscounts = fields.ListField(fields.StringField())
    CategorySpecificationAttributes = fields.ListField(fields.EmbeddedField(CategorySpecificationAttributeSchema))
    Locales = fields.ListField(fields.EmbeddedField(LocaleSchema))
    Active = fields.BooleanField(default=False)
    Deleted = fields.BooleanField(default=False)
    
    class Meta:
        collection_name = "Category"

#Category().delete(conditions={'Name':'test'})
#aaa = Category().find({})

#print(Category.count_documents())
#aaa.delete(conditions={'Name':'test'})
#if aaa.count() > 0:
#	for a in aaa:
#		a.delete()
		
#aaa1= Category()
#aaa1.Name = 'test'
#aaa1.commit()

def delete_test():
	aaa = Category().find({'Name':'test'})
	for a in aaa:
	    a.delete()

def select_all(query={}):
	aaa = Category().find(query)
	#print(aaa.count_documents(query))
	print(Category.count_documents(query))
	for a in aaa:
	    print(a.Name)

def select_insert():
	aaa = Category()
	aaa.Name = 'test'
	aaa.commit()

def select_none():
	if Category.count_documents({'Name':'none'}) == 0:
	    print('None')
	  
#delete_test()
select_all({'Name':'MacBook'})
select_insert()
select_none()

