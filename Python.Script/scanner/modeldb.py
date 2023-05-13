from dataclasses import dataclass
from datetime import datetime
from bson.objectid import ObjectId
from datetime import timedelta

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

@dataclass
class Category:
    _id: str=None
    GenericAttributes=[]
    Name: str=None
    SeName: str=None
    Description: str=None
    BottomDescription: str=None
    CategoryTemplateId: str=None
    MetaKeywords=None
    MetaDescription: str=None
    MetaTitle: str=None
    ParentCategoryId: str=None
    PictureId: str=None
    PageSize: int=20
    AllowCustomersToSelectPageSize: bool=False
    PageSizeOptions: str="6 3 9"
    PriceRanges: float=None
    ShowOnHomePage: bool=False
    FeaturedProductsOnHomaPage: bool=False
    ShowOnSearchBox: bool=False
    SearchBoxDisplayOrder: int=0
    IncludeInTopMenu: bool=False
    SubjectToAcl: bool=False
    CustomerRoles=[]
    Stores= []
    LimitedToStores: bool=False
    Published: bool=True
    DisplayOrder: int=0
    Flag: str=None
    FlagStyle: str=None
    Icon: str=None
    DefaultSort: int=5
    HideOnCatalog: bool=False
    CreatedOnUtc: str=datetime.now()
    UpdatedOnUtc: str=datetime.now()
    AppliedDiscounts=[]
    CategorySpecificationAttributes= []
    Locales= []

    def __post_init__(self):
        self._id = str(ObjectId())

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