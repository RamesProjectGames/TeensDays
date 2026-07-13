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
    public const string diamond10 = "diamond10";
    public const string diamond25 = "diamond25";
    public const string diamond70 = "diamond70";
    public const string diamond150 = "diamond150";
    public const string diamond320 = "diamond320";
    public const string diamond850 = "diamond850";
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
            new ProductDefinition(diamond10, ProductType.Consumable),
            new ProductDefinition(diamond25, ProductType.Consumable),
            new ProductDefinition(diamond70, ProductType.Consumable),
            new ProductDefinition(diamond150, ProductType.Consumable),
            new ProductDefinition(diamond320, ProductType.Consumable),
            new ProductDefinition(diamond850, ProductType.Consumable)
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
            if (productId == diamond10)
            {
                diamondPurchase = 10 * quantity;
            }
            else if (productId == diamond25)
            {
                diamondPurchase = 25 * quantity;
            }
            
            else if (productId == diamond70)
            {
                diamondPurchase = 70 * quantity;
            }
            
            else if (productId == diamond150)
            {
                diamondPurchase = 150 * quantity;
            }
            
            else if (productId == diamond320)
            {
                diamondPurchase = 320 * quantity;
            }
            else if (productId == diamond850)
            {
                diamondPurchase = 850 * quantity;
            }
            // else if(productId == boostPet)
            // {
            //     // PetInfoPanelMenuHandler.Instance.SetMaxStatFromIAP(BoostType, BoostAnimal);
            // }

            bool isFirstPurchase = IsFirstPurchase(productId);
            int firstPurchaseBonus = GetFirstPurchaseBonus(productId, isFirstPurchase);
            int totalDiamondPurchase = diamondPurchase + firstPurchaseBonus;

            if (isFirstPurchase)
            {
                TrackFirstPurchase(productId);
            }

            if(productId.ToLower().Contains("diamond"))
            {
                purchaseDetailsPanel.ShowPurchaseStateInfo(true, $"{totalDiamondPurchase}");
                GameManager.Instance.playerData.currDiamond += totalDiamondPurchase;
            }
            GameManager.Instance.SavePlayerDataToCloud();
            AllButtonInteractable(true);            
        }
    }

    private bool IsFirstPurchase(string productId)
    {
        if (string.IsNullOrEmpty(productId) || GameManager.Instance?.playerData == null)
            return false;

        if (GameManager.Instance.playerData.firstPurchase == null)
            GameManager.Instance.playerData.firstPurchase = new List<string>();

        return !GameManager.Instance.playerData.firstPurchase.Contains(productId);
    }

    private int GetFirstPurchaseBonus(string productId, bool isFirstPurchase)
    {
        if (!isFirstPurchase)
            return 0;

        switch (productId)
        {
            case diamond10:
                return 0;
            case diamond25:
                return 2;
            case diamond70:
                return 10;
            case diamond150:
                return 30;
            case diamond320:
                return 80;
            case diamond850:
                return 200;
            default:
                return 0;
        }
    }

    private void TrackFirstPurchase(string productId)
    {
        if (string.IsNullOrEmpty(productId) || GameManager.Instance?.playerData == null)
            return;

        if (GameManager.Instance.playerData.firstPurchase == null)
            GameManager.Instance.playerData.firstPurchase = new List<string>();

        if (!GameManager.Instance.playerData.firstPurchase.Contains(productId))
        {
            GameManager.Instance.playerData.firstPurchase.Add(productId);
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
            case IAPProductKey.diamond10:
                storeController.PurchaseProduct(diamond10);
                break;
            case IAPProductKey.diamond25:
                storeController.PurchaseProduct(diamond25);
                break;
            case IAPProductKey.diamond70:
                storeController.PurchaseProduct(diamond70);
                break;
            case IAPProductKey.diamond150:
                storeController.PurchaseProduct(diamond150);
                break;
            case IAPProductKey.diamond320:
                storeController.PurchaseProduct(diamond320);
                break;
            case IAPProductKey.diamond850:
                storeController.PurchaseProduct(diamond850);
                break;
            // case IAPProductKey.boostMaxStats:
            //     storeController.PurchaseProduct(boostPet);
            //     break;
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
