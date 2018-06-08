﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Newtonsoft.Json;

namespace QuartersSDK {
    public class QuartersIAP : MonoBehaviour, IStoreListener  {

        /// <summary>
        /// Loaded product lists with definitions and meta data. Can be used to populate UI Shop with ready to purchase products
        /// </summary>
        public List<Product> products = new List<Product>();

        public bool AreProductsLoaded {
            get {
                return products.Count > 0;
            }
        }

        public IStoreController controller;
        private IExtensionProvider extensions;

        public delegate void OnProductsLoadedDelegate (Product[] products);
        public OnProductsLoadedDelegate OnProductsLoaded;

        public delegate void OnInitializeFailedDelegate (InitializationFailureReason reason);
        public OnInitializeFailedDelegate OnInitializeFailedEvent;

        public delegate void PurchaseSucessfull(Product product);
        public event PurchaseSucessfull PurchaseSucessfullDelegate;


        public delegate void PurchaseFailed(string error);
        public PurchaseFailed PurchaseFailedDelegate;



        public bool IsQuartersProduct(Product product) {
            return product.definition.id.Contains(Constants.QUARTERS_PRODUCT_KEY);
        }



        public int ParseQuartersQuantity(Product product) {
            if (!IsQuartersProduct(product)) {
                Debug.LogError("Trying to parse non Quarters product quantity");
            }

            return int.Parse(product.definition.id.Replace(Constants.QUARTERS_PRODUCT_KEY, ""));
        }



        public void Initialize(List<string> productIds, OnProductsLoadedDelegate onProductsLoaded, OnInitializeFailedDelegate onInitializeFailed) {

            Debug.Log("Initialize products: " + JsonConvert.SerializeObject(productIds, Formatting.Indented));

            this.OnProductsLoaded = onProductsLoaded;
            this.OnInitializeFailedEvent = onInitializeFailed;


            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);


            foreach (string productId in productIds) {
                Debug.Log("Initializing product: " + productId);
                builder.AddProduct(productId, ProductType.Consumable);
            }

            UnityPurchasing.Initialize (this, builder);
        }




        public void OnInitialized (IStoreController controller, IExtensionProvider extensions) {
            this.controller = controller;
            this.extensions = extensions;

            Debug.Log("OnInitialized with products count: " + controller.products.all.Length);

            Debug.Log("Available items:");
            foreach (var item in controller.products.all)
            {
                if (item.availableToPurchase)
                {
                    Debug.Log(string.Join(" - ",
                                          new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt
                    }));
                }
            }

            this.products = new List<Product>(controller.products.all);

            OnProductsLoaded(controller.products.all);
        }



        public void OnInitializeFailed (InitializationFailureReason error) {
            Debug.Log("OnInitializeFailed: " + error.ToString());

            if (OnInitializeFailedEvent != null) OnInitializeFailedEvent(error);
        }





        #region Purchasing


        public void BuyProduct(Product product, PurchaseSucessfull purchaseSucessfullDelegate, PurchaseFailed purchaseFailedDelegate ) {
                if (!IsQuartersProduct(product)) return;

                Debug.Log("Buying Quarters: " + product.definition.storeSpecificId);

                #if UNITY_IOS || UNITY_ANDROID

                    this.PurchaseSucessfullDelegate = purchaseSucessfullDelegate;
                    this.PurchaseFailedDelegate = purchaseFailedDelegate;
                    controller.InitiatePurchase(product);

                #elif
                    Debug.LogError("Purchasing quarters through IAP is not supported on this platform
                #endif
            }




         


            public void OnPurchaseFailed (Product i, PurchaseFailureReason p) {

                Debug.Log("OnPurchaseFailed : " + p.ToString());
                Debug.Log(JsonConvert.SerializeObject(i));

                ProductMetadata metaData = i.metadata;

                if (this.PurchaseFailedDelegate != null) this.PurchaseFailedDelegate(p.ToString());

            }





            public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e){

                //TODO validate purchase with playfab





                return PurchaseProcessingResult.Complete;
            }





        #endregion
    	
    }
}
