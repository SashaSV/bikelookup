
Vue.component('footer-component', {
    data: function () {
        return {
            labelCopyright: "Copyright © 2021 BikeLookUp<br /> Все права защищены"
        }
    },
    template: '<div class="footer">\n' +
        '<p class="label-copyright" v-html="labelCopyright"></p>\n' +
        '</div>'
});

Vue.component('findmini', {
    data: function () {
        return {
            Placeholder: "Что мы ищем сегодня?",
            TextToSearch: "",
            HasAny: false,
            Items: [],
            showResults: false
        }
    },
    methods: {
        lookup: function() {
            console.debug(this.TextToSearch);
            axios({
                url: '/catalog/searchtermautocomplete',
                method: 'get',
                params: {
                    term: this.TextToSearch,
                    categoryId: ""}}).then(function (response) 
            {
                if (response.data) {
                    this.Items = response.data
                    this.showResults = true;
                }else {
                    this.Items = [];
                    this.showResults = false;
                }
            }.bind(this));
        },
        onoverlayclick: function ()
        {
            this.showResults = false;
            this.TextToSearch = "";
            this.Items = [];
        }
        },
            template: 
            ` <div id="searchbar">  
                <div class="find-mini">
                   <div class="search-box">
                      <input class="search-input" type="text" v-bind:placeholder="Placeholder" v-model="TextToSearch" v-on:input="lookup()">
                      <div class="search-underline"> </div>
                      <ul v-if="showResults" class="autocomplete-results">
                        <li v-for="item in Items" :key="item.Label">{{item.Label}}</li>
                     </ul>
                   </div>
                </div>
                <div class="magnifying-glass-1">
                   <div class="group"> </div>
                </div>
                 <div v-if="showResults" class="overlay" @click="onoverlayclick()"><div>
              </div>`
});
Vue.component('service-buttons', {
    data: function () {
        return {
            btn: ""
        }
        
    },
    methods: {
        gotowishlist: function() {
            window.location.href = "/wishlist/";
        },
        gotocompare: function()
        {
            window.location.href = "/compareproducts/";
        },
        gotochat: function ()
        {
            window.location.href = "/contactus/";
        }
    },
    template: 
        '<div class="service-buttons">'+
        '<div class="chat" v-on:click="gotochat"><svg class="chat_icon"><path class="chat_icon" xmlns="http://www.w3.org/2000/svg" d="M12.0696 9.54995e-05C17.0538 0.03692 21.4984 3.14163 23.2411 7.80465C24.9839 12.4681 23.6624 17.7217 19.9197 21.0095C16.1775 24.2967 10.7915 24.9364 6.38146 22.6176L6.32649 22.5983C6.31792 22.5933 6.31653 22.5891 6.31469 22.5833L5.49565 22.1198C5.32884 22.0516 5.14399 22.0405 5.03113 22.0691C4.17972 22.3752 3.30957 22.6265 2.42601 22.8216L2.26405 22.8411C1.29139 22.8627 0.801307 22.2316 0.801307 21.3131L0.824757 21.1164C1.04564 20.2029 1.32245 19.3038 1.6375 18.4712C1.68637 18.3136 1.6701 18.1431 1.58577 17.985L1.36518 17.555C-0.584541 13.8272 -0.437507 9.3523 1.75271 5.76012C3.94265 2.16841 7.85562 -0.0166446 12.0696 9.54995e-05ZM12.058 1.67356L11.721 1.67883C8.22593 1.77883 5.00806 3.63745 3.18235 6.63179C1.29801 9.72229 1.17152 13.5719 2.85195 16.7848L3.06906 17.2083C3.35782 17.7486 3.41823 18.3819 3.22067 19.0141C2.91365 19.8296 2.65703 20.6631 2.45227 21.5099L2.5656 21.0684L3.03928 20.9449C3.20187 20.9001 3.36487 20.853 3.52872 20.8033L4.02326 20.6466L4.5256 20.4741C5.05625 20.3279 5.62044 20.3617 6.17623 20.5908C6.29289 20.6471 6.43765 20.7252 6.62013 20.8283L7.12691 21.1191C7.1355 21.1214 7.14301 21.1235 7.14875 21.1251L7.13395 21.1231L7.47923 21.2961C11.1099 23.0492 15.4352 22.5336 18.5504 19.9759L18.8146 19.7515C22.0351 16.9225 23.172 12.4029 21.6726 8.39082C20.173 4.37834 16.3477 1.70618 12.058 1.67356ZM6.73915 10.6315C7.49527 10.6315 8.10878 11.244 8.10878 12.0002C8.10878 12.7565 7.49527 13.369 6.73915 13.369C5.98303 13.369 5.36952 12.7565 5.36952 12.0002C5.36952 11.244 5.98303 10.6315 6.73915 10.6315ZM12.0634 10.6315C12.8195 10.6315 13.433 11.244 13.433 12.0002C13.433 12.7565 12.8195 13.369 12.0634 13.369C11.3073 13.369 10.6938 12.7565 10.6938 12.0002C10.6938 11.244 11.3073 10.6315 12.0634 10.6315ZM17.3877 10.6315C18.1438 10.6315 18.7573 11.244 18.7573 12.0002C18.7573 12.7565 18.1438 13.369 17.3877 13.369C16.6315 13.369 16.018 12.7565 16.018 12.0002C16.018 11.244 16.6315 10.6315 17.3877 10.6315Z" fill="white"/></svg></div>'+
        '<div class="heart" v-on:click="gotowishlist"><svg class="heart_icon"><path class="heart_icon" xmlns="http://www.w3.org/2000/svg" d="M5.66287 0.398262C7.88261 -0.31575 10.2845 -0.0588126 12.2943 1.08156L12.627 1.28121L12.9657 1.08044C14.8711 0.00164358 17.1314 -0.286627 19.2585 0.298398L19.5941 0.398336C24.2333 1.89195 26.3105 6.95749 24.7499 11.8044C23.9675 14.0591 22.6843 16.1033 20.9904 17.7883C18.6855 20.0267 16.1447 22.0035 13.417 23.6807L13.1275 23.8607C12.8334 24.0435 12.4618 24.0466 12.1648 23.8688L11.8546 23.6831C9.1232 22.0035 6.58233 20.0267 4.26952 17.7805C2.58359 16.1033 1.30038 14.0591 0.511351 11.7847C-1.04354 6.95455 1.02507 1.89142 5.66287 0.398262ZM11.8417 2.97382C10.1944 1.84786 8.12629 1.54533 6.22931 2.15552C2.63679 3.31215 1.0081 7.29854 2.26367 11.1996C2.95423 13.1896 4.08638 14.9931 5.56524 16.4644C7.77316 18.6087 10.2069 20.5021 12.8135 22.1051L12.6332 21.9914L13.2242 21.6197C15.2874 20.2875 17.2308 18.7758 19.0277 17.1058L19.6947 16.4722C21.1815 14.9931 22.3136 13.1896 22.9976 11.2194C24.2586 7.30227 22.6228 3.31306 19.0283 2.15582C17.0365 1.51683 14.8623 1.88282 13.1823 3.14053C12.8598 3.382 12.4177 3.38667 12.0902 3.15206L11.8417 2.97382ZM17.5203 5.03209C19.1895 5.56486 20.3732 7.05927 20.5209 8.81498C20.5637 9.32299 20.1861 9.76943 19.6777 9.81214C19.1693 9.85484 18.7225 9.47764 18.6797 8.96963C18.5944 7.95541 17.9134 7.0956 16.9581 6.7907C16.4721 6.63557 16.2039 6.11614 16.3592 5.63051C16.5144 5.14489 17.0343 4.87696 17.5203 5.03209Z" fill="white"/></svg>></div>'+
        '<div class="swap" v-on:click="gotocompare"><svg class="swap_icon"><path class="swap_icon" xmlns="http://www.w3.org/2000/svg" d="M18.3469 3.65269C18.8122 3.65269 19.1967 4.03243 19.2576 4.52513L19.266 4.6621L19.2658 20.5435L22.6926 16.765C23.0507 16.3699 23.6327 16.3684 23.9925 16.7617C24.3195 17.1193 24.3504 17.68 24.0842 18.0758L23.9955 18.1893L18.9981 23.7029C18.968 23.7354 18.9374 23.7649 18.905 23.7921L18.9981 23.7029C18.9531 23.7526 18.9044 23.7961 18.8531 23.8334C18.8352 23.8456 18.8166 23.8581 18.7976 23.8699C18.78 23.8816 18.762 23.8919 18.7438 23.9014C18.7259 23.9101 18.7077 23.9189 18.6892 23.9271C18.6657 23.9381 18.6416 23.9475 18.6172 23.9557C18.6028 23.9599 18.5881 23.9645 18.5733 23.9686C18.5468 23.9766 18.5202 23.9826 18.4934 23.9873C18.4809 23.9889 18.4678 23.9909 18.4546 23.9926C18.4244 23.9971 18.3944 23.9994 18.3645 24C18.3586 23.9994 18.3527 23.9994 18.3469 23.9994L18.329 24C18.299 23.9994 18.2691 23.9971 18.2393 23.9933L18.3469 23.9994C18.2968 23.9994 18.2476 23.995 18.1998 23.9866C18.1732 23.9826 18.1466 23.9766 18.1203 23.9692C18.1064 23.9647 18.0924 23.9604 18.0785 23.9558C18.0522 23.9477 18.026 23.9375 18.0002 23.926C17.9861 23.9189 17.9725 23.9123 17.959 23.9054C17.9379 23.8955 17.9166 23.8836 17.8958 23.8707C17.877 23.8581 17.8584 23.8456 17.8403 23.8325C17.8262 23.8231 17.8122 23.8124 17.7985 23.8012L17.7887 23.7921C17.7564 23.7649 17.7257 23.7354 17.6969 23.7038L17.6953 23.7029L12.698 18.1893C12.3399 17.7941 12.3412 17.155 12.701 16.7617C13.028 16.4042 13.5387 16.3729 13.8979 16.667L14.0008 16.765L17.4276 20.5462L17.4277 4.6621C17.4277 4.10462 17.8392 3.65269 18.3469 3.65269ZM6.17951 0.000560282L6.19741 0C6.22736 0.00063196 6.25729 0.00286205 6.28708 0.00669027L6.17951 0.000560282C6.22958 0.000560282 6.27872 0.00495744 6.32661 0.0134193C6.35281 0.0173516 6.37911 0.0232949 6.40518 0.0305102C6.4205 0.0354483 6.43613 0.0402639 6.45158 0.045515C6.47571 0.0529872 6.49907 0.06207 6.52212 0.0722199C6.5388 0.0803714 6.55568 0.0884734 6.57228 0.0971058C6.59202 0.106478 6.61145 0.117507 6.63059 0.129348C6.64697 0.140425 6.66319 0.151217 6.67907 0.162532C6.69541 0.173203 6.71178 0.185693 6.72784 0.198832L6.83107 0.297114L11.8284 5.81074C12.1865 6.20585 12.1852 6.84498 11.8254 7.23827C11.4983 7.5958 10.9876 7.62708 10.6284 7.33296L10.5256 7.23497L7.09782 3.45219L7.09864 19.3379C7.09864 19.8954 6.68713 20.3473 6.17951 20.3473C5.71419 20.3473 5.32963 19.9676 5.26877 19.4749L5.26038 19.3379L5.25956 3.45353L1.83373 7.23497C1.50818 7.59416 0.997624 7.62803 0.637186 7.33574L0.533893 7.23827C0.20683 6.88073 0.175985 6.32003 0.442135 5.92418L0.53089 5.81074L5.52822 0.297114L5.58234 0.24261C5.59505 0.230669 5.60806 0.219103 5.62136 0.207926L5.52822 0.297114C5.57331 0.247368 5.62194 0.203864 5.67323 0.166604C5.69112 0.154361 5.70972 0.141863 5.72874 0.130086C5.7464 0.118398 5.76435 0.108142 5.78253 0.0985809C5.80047 0.0898573 5.81868 0.0810547 5.83718 0.0728941C5.86069 0.0618535 5.8848 0.052506 5.90917 0.0442952C5.92399 0.0399236 5.93917 0.0352635 5.9545 0.0310265C5.97919 0.0235785 6.00415 0.0178845 6.02925 0.0133372C6.04435 0.0112638 6.05914 0.00900281 6.07404 0.00713322C6.10242 0.00285255 6.13152 0.000683776 6.16065 2.67436e-05C6.16708 0.000628159 6.17329 0.000560282 6.17951 0.000560282Z" fill="white"/>'+'</svg>></div>'+
        '</div>'
});
Vue.component('login', {
    data: function () {
        return {
            btn: ""
        }
    },
    methods: {
        gotoligin: function ()
        {
            window.location.href = "/login/";
        }
        },
    template: '<div class ="login_area" v-on:click="gotoligin"><svg  class="login" width="30" height="32" viewBox="0 0 30 32" fill="none" xmlns="http://www.w3.org/2000/svg">\n' +
        '<path class="login" d="M22.483 0C26.282 0 29.3813 2.99074 29.5553 6.74667L29.563 7.08V24.904C29.563 28.7108 26.5647 31.8178 22.8011 31.9923L22.467 32H14.651C10.8571 32 7.75972 29.0131 7.5808 25.263L7.57264 24.92V23.4128C7.57264 22.7501 8.1099 22.2128 8.77264 22.2128C9.38015 22.2128 9.88223 22.6642 9.96169 23.25L9.97264 23.4128V24.92C9.97264 27.4118 11.9208 29.4497 14.3762 29.5921L14.651 29.6H22.467C24.9677 29.6 27.0122 27.6448 27.1551 25.1799L27.163 24.904V7.08C27.163 4.58707 25.2153 2.55025 22.7581 2.40794L22.483 2.4H14.6686C12.1666 2.4 10.1234 4.35356 9.98061 6.81849L9.97264 7.0944V8.5872C9.97264 9.24994 9.43538 9.7872 8.77264 9.7872C8.16513 9.7872 7.66305 9.33575 7.58359 8.75003L7.57264 8.5872V7.0944C7.57264 3.28733 10.5697 0.182056 14.3345 0.00771968L14.6686 0H22.483ZM16.4932 10.3679L16.6281 10.4838L21.3129 15.1494C21.3517 15.1887 21.3868 15.2287 21.4191 15.271L21.3129 15.1494C21.372 15.2083 21.4237 15.2718 21.4679 15.3387C21.4834 15.363 21.4989 15.3884 21.5134 15.4143C21.5232 15.4307 21.5319 15.4472 21.5402 15.4639C21.5563 15.4972 21.5715 15.5316 21.5852 15.5668C21.5926 15.585 21.5991 15.6031 21.6052 15.6213C21.6158 15.6547 21.6256 15.6893 21.6339 15.7245C21.6388 15.7436 21.6427 15.7623 21.6461 15.7811C21.6517 15.8135 21.6565 15.8472 21.6598 15.8814C21.6626 15.9054 21.6642 15.929 21.6652 15.9525C21.6653 15.968 21.6656 15.9838 21.6656 15.9997L21.6652 16.0453C21.6643 16.0699 21.6626 16.0945 21.6602 16.119L21.6656 15.9997C21.6656 16.0746 21.6587 16.148 21.6456 16.2191C21.6426 16.2371 21.6387 16.2559 21.6343 16.2745C21.6253 16.3114 21.6151 16.3473 21.6032 16.3825C21.5981 16.3991 21.5921 16.4157 21.5858 16.4321C21.5722 16.4661 21.5575 16.4994 21.5415 16.5318C21.5334 16.5493 21.5239 16.5673 21.5139 16.5852C21.4956 16.6168 21.4766 16.6473 21.4563 16.677C21.4478 16.6902 21.4389 16.7028 21.4296 16.7152C21.3901 16.7668 21.3535 16.8088 21.3141 16.8482L21.3129 16.8499L16.6281 21.5155C16.1585 21.9832 15.3987 21.9817 14.931 21.5121C14.5059 21.0852 14.4685 20.4184 14.818 19.9494L14.9345 19.815L17.5584 17.1984L1.2 17.1997C0.537258 17.1997 0 16.6624 0 15.9997C0 15.3922 0.451446 14.8901 1.03717 14.8106L1.2 14.7997L17.5584 14.7984L14.9345 12.1843C14.5076 11.7592 14.4675 11.0926 14.8151 10.6221L14.931 10.4873C15.3562 10.0604 16.0227 10.0203 16.4932 10.3679Z" fill="white"/>\n' +
        '</svg> </div>>\n'
});
Vue.component('mylocation', {
    data: function () {
        return {
            location_text: "Киев?"
        }
    },
    template:'<div class="my-location"> <div class="location"></div><div class="location-text">{{ location_text }}</div></div>'
});

Vue.component('button-add', {
    data: function () {
        return {
            CreateAdd: "ПОДАТЬ ОБЬЯВЛЕНИЕ"
        }
    },
    template: '<button class="button-ad">  <p class="button-addtext"> {{ CreateAdd }}</p></button>'
});

Vue.component('header-up', {
    data: function () {
        return {
            count: 0
        }
    },
    template: '<div class="header-up1">\n' +
        '    <div class="header-up-inner">\n' +
        '      <LangSelect class="lang-select" />' +
        '<button-add class="button-add"></button-add>' +
        '<findmini class="find-mini"></findmini>' +
        '<mylocation class="my-location"></mylocation>'+
        '      <div class="buttonbar">\n' +
        '        <service-buttons class="service-buttons"></service-buttons>' +
        '        <login class="service-buttons"><login/>\n' +
        '      </div>\n' +
        '    </div>\n' +
        '  </div>'})
if (typeof minPrice === "undefined") 
{
    var minPrice = 0;
    var maxPrice = 0;
    var maxAv = 0;
    var minAv = 0;
}
var vm = new Vue({
    el: '#app',
    data() {
        return {
            show: false,
            fluid: false,
            hover: false,
            active: false,
            NextDropdownVisible: false,
            value: 5,
            searchitems: null,
            searchcategories: null,
            searchmanufacturers: null,
            searchblog: null,
            searchproducts: null,
            minAngle: minPrice,
            maxAngle: maxPrice,
        }
    },
    props: {
        flycart: null,
        flycartitems: null,
        flycartindicator: null,
    },
    mounted() {
        if (localStorage.fluid == "true") this.fluid = "fluid";
        if (localStorage.fluid == "fluid") this.fluid = "fluid";
        if (localStorage.fluid == "") this.fluid = "false";
        this.updateFly();
    },
    watch: {
        fluid(newName) {
            localStorage.fluid = newName;
        },
    },
    computed: {
        sliderMin: {
            get: function () {
                var val = parseInt(this.minAngle);
                return val;
            },
            set: function (val) {
                val = parseInt(val);
                if (val > this.maxAngle) {
                    this.maxAngle = val;
                }
                this.minAngle = val;
            }
        },
        sliderMax: {
            get: function () {
                var val = parseInt(this.maxAngle);
                return val;
            },
            set: function (val) {
                val = parseInt(val);
                if (val < this.minAngle) {
                    this.minAngle = val;
                }
                this.maxAngle = val;
            }
        }
    },
    methods: {
        applyPriceFilter()
        {
            let params = new URLSearchParams(window.location.search);
            params.set("price",this.minAngle + '-'+ this.maxAngle);
            window.location.search = params;
        },
        onPriceFilter()
        {
            if(parseInt(this.maxAngle) > parseInt(maxAv))
            {
                this.maxAngle = parseInt(maxAv);
            }
            if(parseInt(this.minAngle) < parseInt(minAv))
            {
                this.minAngle = parseInt(minAv);
            }
            if(parseInt(this.minAngle) > parseInt(maxAv))
            {
                this.minAngle = parseInt(maxAv);
            }
            if(parseInt(this.maxAngle )< parseInt(minAv))
            {
                this.maxAngle = parseInt( minAv);
            }
        },
        updateFly() {
            axios({
                baseURL: '/Component/Index?Name=FlyoutShoppingCart',
                method: 'get',
                data: null,
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Response-View': 'Json'
                }
            }).then(response => (
                this.flycart = response.data,
                this.flycartitems = response.data.Items,
                this.flycartindicator = response.data.TotalProducts
            ))
        },
        showModalBackInStock() {
            this.$refs['back-in-stock'].show()
        },
        productImage: function (event) {
            var Imagesrc = event.target.parentElement.getAttribute('data-href');
            function collectionHas(a, b) {
                for (var i = 0, len = a.length; i < len; i++) {
                    if (a[i] == b) return true;
                }
                return false;
            }
            function findParentBySelector(elm, selector) {
                var all = document.querySelectorAll(selector);
                var cur = elm.parentNode;
                while (cur && !collectionHas(all, cur)) {
                    cur = cur.parentNode;
                }
                return cur;
            }

            var yourElm = event.target
            var selector = ".product-box";
            var parent = findParentBySelector(yourElm, selector);
            var Image = parent.querySelectorAll(".main-product-img")[0];
            Image.setAttribute('src', Imagesrc);
        },
        validateBeforeSubmit(event) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    event.srcElement.submit();
                    return
                } else {
                    if (vm.$refs.selected !== undefined && vm.$refs.selected.checked) {
                        event.srcElement.submit();
                        return
                    }
                    if (vm.$refs.visible !== undefined && vm.$refs.visible.style.display == "none") {
                        event.srcElement.submit();
                        return
                    }
                }
            });
        },
        validateBeforeClick(event) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    var callFunction = event.srcElement.getAttribute('data-click');
                    eval(callFunction)
                    return
                }
            });
        },
        validateBeforeSubmitParam(event, param) {
            this.$validator.validateAll().then((result) => {
                if (result) {
                    var para = document.createElement("input");
                    para.name = param;
                    para.type = 'hidden';
                    event.srcElement.appendChild(para);
                    event.srcElement.submit();
                    return
                } else {
                    if ((vm.$refs.selected !== undefined && vm.$refs.selected.checked) ||
                        (vm.$refs.visible !== undefined && vm.$refs.visible.style.display == "none")) {
                        var para = document.createElement("input");
                        para.name = param;
                        para.type = 'hidden';
                        event.srcElement.appendChild(para);
                        event.srcElement.submit();
                        return
                    }
                }
            });
        },
        isMobile() {
            return (typeof window.orientation !== "undefined") || (navigator.userAgent.indexOf('IEMobile') !== -1);
        },
    }
});
