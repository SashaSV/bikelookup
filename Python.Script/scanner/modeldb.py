from dataclasses import dataclass
from datetime import datetime
from bson.objectid import ObjectId
from datetime import timedelta
from pymongo import MongoClient
from umongo import Document, EmbeddedDocument, fields, validate
from umongo.frameworks import PyMongoInstance
import pytils.translit

class Database:
	@classmethod
	def initialize(cls, dbconnect):
		client = MongoClient(dbconnect)
		cls.database = client.get_default_database()
		return PyMongoInstance(cls.database)

instance = Database.initialize("mongodb://localhost:27017/bldb")

@dataclass
class Adresses:
    _id:int=str(ObjectId())
    GenericAttributes = []
    FirstName:str=None
    LastName:str=None
    Email:str=None
    Company:str=None
    VatNumber:str=None
    CountryId:str=None
    StateProvinceId:str=None
    City:str=None
    Address1:str=None
    Address2:str=None
    ZipPostalCode:str=None
    PhoneNumber:str=None
    FaxNumber:str=None
    CustomAttributes=None
    CreatedOnUtc:str = datetime.now()
    
    def __post_init__(self):
        self._id = str(ObjectId())


@dataclass
class Venodr:
    _id: str=None
    GenericAttributes=[]
    Name: str=None
    SeName: str=None
    PictureId: str=None
    Email: str=None
    Description: str=None
    StoreId: str=None
    AdminComment: str=None
    Active: bool=True
    Deleted: bool=False
    DisplayOrder: int=0
    MetaKeywords: str=None
    MetaDescription: str=None
    MetaTitle: str=None
    PageSize: int=20
    AllowCustomersToSelectPageSize: bool=True
    PageSizeOptions: int=None
    AllowCustomerReviews: bool=True
    ApprovedRatingSum: int=0
    NotApprovedRatingSum: int=0
    ApprovedTotalReviews: int=0
    NotApprovedTotalReviews: int=0
    Commission: int=0
    Address: Adresses=None
    Locales= []
    VendorNotes= []
    AppliedDiscounts= []
    VendorSpecificationAttributes= []

    def __post_init__(self):
        self._id = str(ObjectId())

# @dataclass
# class Category:
#     _id: str=None
#     GenericAttributes=[]
#     Name: str=None
#     SeName: str=None
#     Description: str=None
#     BottomDescription: str=None
#     CategoryTemplateId: str=None
#     MetaKeywords=None
#     MetaDescription: str=None
#     MetaTitle: str=None
#     ParentCategoryId: str=None
#     PictureId: str=None
#     PageSize: int=20
#     AllowCustomersToSelectPageSize: bool=False
#     PageSizeOptions: str="6 3 9"
#     PriceRanges: float=None
#     ShowOnHomePage: bool=False
#     FeaturedProductsOnHomaPage: bool=False
#     ShowOnSearchBox: bool=False
#     SearchBoxDisplayOrder: int=0
#     IncludeInTopMenu: bool=False
#     SubjectToAcl: bool=False
#     CustomerRoles=[]
#     Stores= []
#     LimitedToStores: bool=False
#     Published: bool=True
#     DisplayOrder: int=0
#     Flag: str=None
#     FlagStyle: str=None
#     Icon: str=None
#     Active: bool=False
#     Deleted: bool=False
#     DefaultSort: int=5
#     HideOnCatalog: bool=False
#     CreatedOnUtc: datetime=datetime.now()
#     UpdatedOnUtc: datetime=datetime.now()
#     AppliedDiscounts=[]
#     CategorySpecificationAttributes= []
#     Locales= []

#     def __post_init__(self):
#         self._id = str(ObjectId())
#         #self.CreatedOnUtc = datetime.now()
#         #self.UpdatedOnUtc = datetime.now()
        
@dataclass
class Picture:
    _id: str=None
    GenericAttributes: list[str] = None
    PictureBinary: str=None
    MimeType: str='image/jpeg'
    SeoFilename: str=None
    UrlImage: str=None
    AltAttribute: str=None
    TitleAttribute: str=None
    IsNew: bool=True

    def __post_init__(self):
        self._id = str(ObjectId())
        self.GenericAttributes=[]
    
@dataclass
class Manufacturer:
    _id: str=None
    GenericAttributes=[]
    Name: str=None
    SeName: str=None
    Description: str=None
    BottomDescription: str=None
    ManufacturerTemplateId: str=None
    MetaKeywords: str=''
    MetaDescription: str=''
    MetaTitle: str=''
    ParentCategoryId: str='' 
    PictureId: str=None
    PageSize: int=20
    AllowCustomersToSelectPageSize: bool=True
    PageSizeOptions: str=None
    PriceRanges: str=None
    ShowOnHomePage: bool=False
    FeaturedProductsOnHomaPage: bool=False
    IncludeInTopMenu: bool=False
    SubjectToAcl: bool=False
    CustomerRoles=[]
    LimitedToStores: bool=False
    Stores =[]
    Published: bool=True
    DisplayOrder: int=0
    Icon: str=None
    DefaultSort: int=5
    HideOnCatalog: bool=False
    CreatedOnUtc: str=datetime.now()
    UpdatedOnUtc: str=datetime.now()
    AppliedDiscounts=[]
    Locales=[]

    def __post_init__(self):
        self._id = str(ObjectId())

@dataclass
class UrlRecord:
    _id: str=None
    GenericAttributes=[]
    EntityId: str=None
    EntityName: str=None
    Slug: str=None
    IsActive: bool=True
    LanguageId: str=None

    def __post_init__(self):
        self._id = str(ObjectId())
        self.GenericAttributes = []

@dataclass
class TierPrice:
    _id: str=None
    StoreId: str=None
    CustomerRoleId: str=None
    Quantity: int=0
    Price: float=0.0
    StartDateTimeUtc: str=datetime.now()
    EndDateTimeUtc: str=None
    
    def __post_init__(self):
        self._id = str(ObjectId())

@dataclass
class Product:
    _id: str=None
    GenericAttributes=[]
    ProductTypeId: int=5 #singl product
    ParentGroupedProductId: str=None
    VisibleIndividually: bool=False
    Name: str=None
    SeName: str=None
    ShortDescription: str=None
    Url: str=None
    ProductTemplateId: str=None
    VendorId: str=None
    Sku: str=None
    RecurringCycleLength: int=100
    Weeldiam: str=None
    ManufactureName: str=None
    Model: str=None
    Year: str=None
    Color: str=None
    Size: str=None
    FullDescription: str=None
    AdminComment: str=None
    ShowOnHomePage: str=False
    MetaKeywords: str=None
    MetaDescription: str=None
    MetaTitle: str=None
    AllowCustomerReviews: str=True
    ApprovedRatingSum: int=0
    NotApprovedRatingSum: int=0
    ApprovedTotalReviews: int=0
    NotApprovedTotalReviews: int=0
    SubjectToAcl: bool=False
    CustomerRoles=[]
    LimitedToStores: bool=False
    Stores=[]
    ExternalId: str=None
    ManufacturerPartNumber: str=None
    Gtin: str=None
    IsGiftCard: bool=False
    GiftCardTypeId: int=0
    OverriddenGiftCardAmount: str=None
    RequireOtherProducts: bool=False
    RequiredProductIds: str=None
    AutomaticallyAddRequiredProducts: bool=False
    IsDownload: bool=False
    DownloadId: str=None
    UnlimitedDownloads: bool=True
    MaxNumberOfDownloads: int=10
    DownloadExpirationDays: str=None
    DownloadActivationTypeId: int=0
    HasSampleDownload: bool=False
    SampleDownloadId: str=None
    HasUserAgreement: bool=False
    UserAgreementText: str=None
    IsRecurring: bool=False
    RecurringCyclePeriodId: int=0
    RecurringTotalCycles: int=10
    IncBothDate: bool=False
    Interval: int=0
    IntervalUnitId: int=0
    IsShipEnabled: bool=False
    IsFreeShipping: bool=False
    ShipSeparately: bool=False
    AdditionalShippingCharge: int=0
    DeliveryDateId: str=None
    IsTaxExempt: bool=False
    TaxCategoryId: str=None
    IsTele: bool=False
    ManageInventoryMethodId: int=0
    UseMultipleWarehouses: bool=False
    WarehouseId: str=None
    StockQuantity: int=0
    DisplayStockAvailability: bool=False
    DisplayStockQuantity: bool=False
    MinStockQuantity: int=0
    LowStock: bool=True
    LowStockActivityId: int=0
    NotifyAdminForQuantityBelow: int=1
    BackorderModeId: int=0
    AllowBackInStockSubscriptions: bool=False
    OrderMinimumQuantity: int=1
    OrderMaximumQuantity: int=1000
    AllowedQuantities: str=None
    AllowAddingOnlyExistingAttributeCombinations: bool=False
    NotReturnable: bool=False
    DisableBuyButton: bool=False
    DisableWishlistButton: bool=False
    AvailableForPreOrder: bool=False
    PreOrderAvailabilityStartDateTimeUtc: str=None
    CallForPrice: bool=False
    Price: float=0.0
    OldPrice: float=0.0
    CatalogPrice: float=0.0
    ProductCost: float=0.0
    CustomerEntersPrice: bool=False
    MinimumCustomerEnteredPrice: float=0.0
    MaximumCustomerEnteredPrice: float=1000
    BasepriceEnabled: bool=False
    BasepriceAmount: int=0
    BasepriceUnitId: str=None
    BasepriceBaseAmount: int=0
    BasepriceBaseUnitId: str=None
    UnitId: str=None
    CourseId: str=None
    MarkAsNew: bool=True
    MarkAsNewStartDateTimeUtc: str=datetime.now()-timedelta(days=1)
    MarkAsNewEndDateTimeUtc: str=datetime.now()+timedelta(days=30)
    Weight: int=0
    Length: int=0
    Width: int=0
    Height: int=0
    AvailableStartDateTimeUtc: str=None
    AvailableEndDateTimeUtc: str=None
    StartPrice: int=0
    HighestBid: int=0
    HighestBidder: str=None
    AuctionEnded: bool=False
    DisplayOrder: int=0
    DisplayOrderCategory: int=0
    DisplayOrderManufacturer: int=0
    Published: bool=True
    CreatedOnUtc: str=datetime.now()
    UpdatedOnUtc: str=datetime.now()
    Sold: int=0
    Viewed: int=0
    OnSale: int=0
    Flag: str=None
    Locales=[]
    ProductCategories: list[str]=None
    ProductManufacturers: list[str]=None
    ProductPictures: list[str]=None
    ProductSpecificationAttributes: list[str]=None
    ProductTags: list[str]=None
    ProductAttributeMappings: list[str]=None
    ProductAttributeCombinations=[]
    TierPrices: list[TierPrice]=None
    AppliedDiscounts=[]
    ProductWarehouseInventory=[]
    CrossSellProduct=[]
    RelatedProducts=[]
    SimilarProducts=[]
    BundleProducts=[]

    def __post_init__(self):
        self._id = str(ObjectId())
        self.ProductCategories = []
        self.ProductManufacturers = []
        self.ProductPictures = []
        self.TierPrices = []
        self.ProductSpecificationAttributes = []
        self.ProductAttributeMappings = []
        self.ProductAttributeCombinations = []
        
@dataclass
class SpecificationAttribute:
    _id: str=None
    GenericAttributes=[]
    Name: str=None
    SeName: str=None
    DisplayOrder: int=0
    Locales: list[str]=None
    SpecificationAttributeOptions=[]
    
    def __post_init__(self):
        self._id = str(ObjectId())

def get_sename(sename, entityId = None, entityName = None, languageId = None):
        #replacechar = [' ', '-', '!', '?', ':', '"', '.', '+']
        okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-"
        okRuChars = "abcdefghijklmnopqrstuvwxyzабвгдеёжзийлкмнопрстуфхцчшщъыьэюя1234567890 _-"
        senamenew_ru = sename.lower().replace('і', 'i')
        senamenew_ru = senamenew_ru.replace('є', 'e')
        senamenew_ru = senamenew_ru.replace('ї', 'i')
        senamenew_ru = senamenew_ru.replace('"', '_inch_')
        senamenew_ru = senamenew_ru.replace('+', '_plus_')
        senamenew_ru = senamenew_ru.replace('-', '_minus_')
        sename_new = ''

        for s in senamenew_ru:
            if okRuChars.__contains__(s):
                sename_new = sename_new + s

        senamenew_ru = sename_new

        senamenew_ru = pytils.translit.translify(senamenew_ru.strip()).lower()

        sename_new = ''
        for s in senamenew_ru:
            if okChars.__contains__(s):
                sename_new = sename_new + s

        sename_new = sename_new.replace(' ', '-')
        sename_new = sename_new.replace('--', '-')
        sename_new = sename_new.replace('__', '_')

        if not entityId is None:
            urlrecord = UrlRecord(EntityId=entityId,
                      EntityName=entityName,
                      Slug=sename_new,
                      LanguageId=languageId)
            
            urlrecord.commit()
        return sename_new

@instance.register
class UrlRecord(Document):
	GenericAttributes = fields.ListField(fields.StrField())
	EntityId = fields.StringField(default=None)
	EntityName = fields.StringField(default=None)
	Slug = fields.StringField(default=None)
	IsActive = fields.BooleanField(default=True)
	LanguageId = fields.StringField(default=None)
    
	class Meta:
		collection_name = "UrlRecord"
                
@instance.register
class Locales(EmbeddedDocument):
    _id = fields.StringField()
    LanguageId = fields.StringField()
    LocaleKey = fields.StringField()
    LocaleValue = fields.StringField()

@instance.register
class CategorySpecificationAttributes(EmbeddedDocument):
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
    Locales = fields.ListField(fields.EmbeddedField(Locales))
    AttributeType = fields.IntegerField()

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
    CreatedOnUtc = fields.DateTimeField(validate=validate.Range(min=datetime.datetime.now()))
    UpdatedOnUtc = fields.DateTimeField(validate=validate.Range(min=datetime.datetime.now()))
    AppliedDiscounts = fields.ListField(fields.StringField())
    CategorySpecificationAttributes = fields.ListField(fields.EmbeddedField(CategorySpecificationAttributes))
    Locales = fields.ListField(fields.EmbeddedField(Locales))
    Active = fields.BooleanField(default=False)
    Deleted = fields.BooleanField(default=False)
    
    class Meta:
        collection_name = "Category"