using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Singleton { get; private set; }
    // public const string diamond200 = "diamond200";
    // public const string diamond500 = "diamond500";
    // public const string diamond1000 = "diamond1000";
    // public const string diamond2000 = "diamond2000";
    // public const string diamond5000 = "diamond5000";
    // public const string diamond10000 = "diamond10000";
    // public const string boostPet = "boostMaxStats";
    public static bool isInitialized { get; private set; } = false;
    private static StoreController storeController;
    
    public int itemPerShelves = 2;
    [Space(10)]
    public GameObject IAPPanel;
    public IAPItemUI IAPButtonPrefab;
    public Transform IAPButtonParent;
    public GameObject confirmationPopUp;
    public IAPPurchaseDetails purchaseDetailsPanel;
    public List<IAPItemUI> IAPItems = new List<IAPItemUI>();
    List<Transform> IAPShelvesList = new List<Transform>();
    IAPProductKey currentProductKey;

    [SerializeField] private bool devMode;
    string BoostType;

    async void Awake()
    {
        if (Singleton != null)
        {
            Destroy(Singleton.gameObject);
        }
        Singleton = this;
    }
    public async void InitializedIAP()
    {
        await InitIAP();        
    }
    private async Task InitIAP()
    {
        try
        {
            var option = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(option);

            storeController = UnityIAPServices.StoreController();
            storeController.OnStoreDisconnected += StoreDisconnected;
            storeController.OnProductsFetched += ProductsFetched;
            storeController.OnProductsFetchFailed += ProductsFetchFailed;
            storeController.OnPurchasesFetched += PurchaseFetched;
            storeController.OnPurchasesFetchFailed += PurchaseFetchFailed;
            storeController.OnPurchasePending += PurchasePending;
            storeController.OnPurchaseConfirmed += PurchaseConfirmed;
            storeController.OnPurchaseFailed += PurchaseFailed;
            storeController.OnPurchaseDeferred += PurchaseDeffered;

            RegisterEntitlementCallback();

            await storeController.Connect();

            var initialProductToFetch = BuildProductDefinitions();
            storeController.FetchProducts(initialProductToFetch);
        }
        catch (Exception ex)
        {

            Debug.Log($"Initiliazation Failed, cause : {ex}");
        }
    }

    private List<ProductDefinition> BuildProductDefinitions()
    {
        var initialProductToFetch = new List<ProductDefinition>
        {
            // new ProductDefinition(diamond200, ProductType.Consumable),
            // new ProductDefinition(diamond500, ProductType.Consumable),
            // new ProductDefinition(diamond1000, ProductType.Consumable),
            // new ProductDefinition(diamond2000, ProductType.Consumable),
            // new ProductDefinition(diamond5000, ProductType.Consumable),
            // new ProductDefinition(diamond10000, ProductType.Consumable),
            // new ProductDefinition(boostPet, ProductType.Consumable)
        };
        return initialProductToFetch;
    }
    private void RegisterEntitlementCallback()
    {
        storeController.OnCheckEntitlement += (result) =>{
            Product product = result.Product;
            var status = result.Status;

            // Debug.Log($"Product is {product}. Entitle Status is {status}");

            bool isEntitled = status == EntitlementStatus.FullyEntitled;
            if(isEntitled)
            {
                // Set No Comsumable Product here (i.e no ads / diamond pass)
            }
        };
    }
    #region  Product Initiliaze
    private void StoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.Log($"Initialization/ Connection Failed, cause : {description}");
    }
    private void ProductsFetched(List<Product> products)
    {
        storeController.FetchPurchases();
        for (int i = 0; i < products.Count; i++)
        {
            var product = products[i];
            IAPProductKey productKey = (IAPProductKey)Enum.Parse(typeof(IAPProductKey), product.definition.id);
            // if(productKey == IAPProductKey.boostMaxStats)
            // {
            //     continue;
            // }
            string price = product.metadata.localizedPrice + " " + product.metadata.isoCurrencyCode;
            string description = product.metadata.localizedDescription;
            string reward = Regex.Replace(product.definition.id, @"\D", ""); ;
            if (i % itemPerShelves == 0)
            {
                GameObject IAPShelves = Instantiate(IAPPanel, IAPButtonParent);
                IAPShelvesList.Add(IAPShelves.transform);
            }
            //Set button UI for each products            
            var currentShelves = Mathf.FloorToInt(i / itemPerShelves);
            GameObject newIAPButton = Instantiate(IAPButtonPrefab.gameObject, IAPShelvesList[currentShelves]);
            IAPItemUI iAPItemUI = newIAPButton.GetComponent<IAPItemUI>();
            iAPItemUI.Initilized(price, description, reward, true,productKey);
            // Set Icon for each product
            iAPItemUI.SetClickButton(() =>
            {
                currentProductKey = productKey;
            });
            IAPItems.Add(iAPItemUI);
        }
    }
    private void ProductsFetchFailed(ProductFetchFailed failed)
    {
        Debug.Log($"Failed to fetch products, cause : {failed}");
    }
    private void PurchaseFetched(Orders orders)
    {
        isInitialized = true;

        foreach (var product in storeController.GetProducts())
        {
            storeController.CheckEntitlement(product);
        }
    }
    private void PurchaseFetchFailed(PurchasesFetchFailureDescription description)
    {
        Debug.Log($"Failed to fetch purchases, cause : {description}");
    }
    private void PurchaseConfirmed(Order order)
    {
        if(order?.Info?.PurchasedProductInfo != null && order.Info.PurchasedProductInfo.Count > 0)
        {
            string productId = order.Info.PurchasedProductInfo[0].productId;
            int diamondPurchase = 0;
            int quantity = GetPurchaseQuantity(order);
            // if (productId == diamond200)
            // {
            //     diamondPurchase = 200 * quantity;
            // }
            // else if (productId == diamond500)
            // {
            //     diamondPurchase = 500 * quantity;
            // }
            
            // else if (productId == diamond1000)
            // {
            //     diamondPurchase = 1000 * quantity;
            // }
            
            // else if (productId == diamond2000)
            // {
            //     diamondPurchase = 2000 * quantity;
            // }
            
            // else if (productId == diamond5000)
            // {
            //     diamondPurchase = 5000 * quantity;
            // }
            // else if (productId == diamond10000)
            // {
            //     diamondPurchase = 10000 * quantity;
            // }
            // else if(productId == boostPet)
            // {
            //     // PetInfoPanelMenuHandler.Instance.SetMaxStatFromIAP(BoostType, BoostAnimal);
            // }
            if(productId.ToLower().Contains("diamond"))
            {
                purchaseDetailsPanel.ShowPurchaseStateInfo(true, $"{diamondPurchase}");
                GameManager.Instance.playerData.currDiamond += diamondPurchase;
            }
            GameManager.Instance.SavePlayerDataToCloud();
            AllButtonInteractable(true);            
        }
    }

    private int GetPurchaseQuantity(Order order)
    {
        int quantity = 1;
        string receipt = order.Info.Receipt;
        if(!string.IsNullOrEmpty(receipt))
        {
            var payData = JsonUtility.FromJson<IAPPayData>(receipt);
            if(payData.Store != "fake")
            {
                IAPPayload payload = JsonUtility.FromJson<IAPPayload>(payData.Payload);
                IAPPayloadData payloadData = JsonUtility.FromJson<IAPPayloadData>(payload.json);
                quantity = payloadData.quantity;
            }
        }
        return quantity;
    }

    private void PurchaseFailed(FailedOrder order)
    {
        if (order?.Info?.PurchasedProductInfo == null || order.Info.PurchasedProductInfo.Count == 0)
        {
            Debug.Log($"Purchase Failed but no product available");
            return;
        }
        var productId = order.Info.PurchasedProductInfo[0].productId;
        var reason = order.FailureReason;
        var message = order.Details;
        
        purchaseDetailsPanel.ShowPurchaseStateInfo(false);
        
        Debug.Log($"Purchase Failed. Product is {productId}. Reason is {reason}. Here is messages {message}");
        // Play purchase sound effect on failed purchase
        AllButtonInteractable(true);
    }    

    private void PurchasePending(PendingOrder order)
    {
        Debug.Log($"Purchase Pending : {order}");
        storeController.ConfirmPurchase(order);
    }
    private void PurchaseDeffered(DeferredOrder order)
    {
        Debug.Log($"Purchase Deffered : {order?.Info}");
        //Show UI like purchase pending Approval
        AllButtonInteractable(false);
    }
    #endregion

    public void BuyProduct()
    {
        if (!isInitialized)
        {
            Debug.Log("IAP Module is not initialized");
            return;
        }
        switch (currentProductKey)
        {
            // case IAPProductKey.diamond200:
            //     storeController.PurchaseProduct(diamond200);
            //     break;
            // case IAPProductKey.diamond500:
            //     storeController.PurchaseProduct(diamond500);
            //     break;
            // case IAPProductKey.diamond1000:
            //     storeController.PurchaseProduct(diamond1000);
            //     break;
            // case IAPProductKey.diamond2000:
            //     storeController.PurchaseProduct(diamond2000);
            //     break;
            // case IAPProductKey.diamond5000:
            //     storeController.PurchaseProduct(diamond5000);
            //     break;
            // case IAPProductKey.diamond10000:
            //     storeController.PurchaseProduct(diamond10000);
            //     break;
            // case IAPProductKey.boostMaxStats:
            //     storeController.PurchaseProduct(boostPet);
            //     break;
            case 0:
                break;
        }
        ShowConfirmationPopUp(false);
    }
    public void AllButtonInteractable(bool isInteractable)
    {
        foreach (var item in IAPItems)
        {
            item.SetInteractable(isInteractable);
        }
    }
    public void ShowConfirmationPopUp(bool isShow)
    {
        if(isShow)
            confirmationPopUp.LeanScale(Vector3.one, 0.3f).setEaseOutBack();
        else
            confirmationPopUp.LeanScale(Vector3.zero, 0.3f).setEaseInBack();
    }

    public void AccessIAP()
    {
        if(devMode)
        {
            // PetProfileManager.Singleton.playerProfile.AddMoney(67676767676767.67m - PetProfileManager.Singleton.playerProfile.GetMoney());
            // PetProfileManager.Singleton.playerProfile.AddPremCurrency(67676767676767.67m - PetProfileManager.Singleton.playerProfile.GetPremCurrency());
        }
        else
        {
            //Open IN App Purcahse Shop Display
        }
    }
}
