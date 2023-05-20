#import pymongo
import datetime as dt
from pymongo import MongoClient
from umongo import Document, fields, validate
from umongo.frameworks import PyMongoInstance

class Database:
	@classmethod
	def initialize(cls, dbconnect):
		client = MongoClient(dbconnect)
		cls.database = client.get_default_database()
		return PyMongoInstance(cls.database)
	
    # @classmethod
    # def load_from_db(cls, query):
    #     return cls.database.Category.find(query)

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
class LocaleSchema(Document):
    LanguageId = fields.StringField()
    LocaleKey = fields.StringField()
    LocaleValue = fields.StringField()

@instance.register
class CategorySpecificationAttributeSchema(Document):
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
    Locales = fields.ListField(fields.ReferenceField("LocaleSchema"))
    AttributeType = fields.IntegerField()

@instance.register
class Category(Document):
    GenericAttributes = fields.ListField(fields.StringField())
    Name = fields.StringField()
    SeName = fields.StringField()
    Description = fields.StringField()
    BottomDescription = fields.StringField()
    CategoryTemplateId = fields.StringField()
    MetaKeywords = fields.StringField()
    MetaDescription = fields.StringField()
    MetaTitle = fields.StringField()
    ParentCategoryId = fields.StringField()
    PictureId = fields.StringField()
    PageSize = fields.IntegerField()
    AllowCustomersToSelectPageSize = fields.BooleanField()
    PageSizeOptions = fields.StringField()
    PriceRanges = fields.NumberField()
    ShowOnHomePage = fields.BooleanField()
    FeaturedProductsOnHomaPage = fields.BooleanField()
    ShowOnSearchBox = fields.BooleanField()
    SearchBoxDisplayOrder = fields.IntegerField()
    IncludeInTopMenu = fields.BooleanField()
    SubjectToAcl = fields.BooleanField()
    CustomerRoles = fields.ListField(fields.StringField())
    Stores = fields.ListField(fields.StringField())
    LimitedToStores = fields.BooleanField()
    Published = fields.BooleanField()
    DisplayOrder = fields.IntegerField()
    Flag = fields.StringField()
    FlagStyle = fields.StringField()
    Icon = fields.StringField()
    DefaultSort = fields.IntegerField()
    HideOnCatalog = fields.BooleanField()
    CreatedOnUtc = fields.DateTimeField(validate=validate.Range(min=dt.datetime(1900, 1, 1)))
    UpdatedOnUtc = fields.DateTimeField(validate=validate.Range(min=dt.datetime(1900, 1, 1)))
    AppliedDiscounts = fields.ListField(fields.StringField())
    CategorySpecificationAttributes = fields.ListField(fields.ReferenceField("CategorySpecificationAttributeSchema"))
    Locales = fields.ListField(fields.ReferenceField("LocaleSchema"))
    Active = fields.BooleanField()
    Deleted = fields.BooleanField()
    
    class Meta:
        collection_name = "Category"


