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
