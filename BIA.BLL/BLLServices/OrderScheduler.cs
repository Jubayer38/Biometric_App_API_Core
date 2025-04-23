using BIA.BLL.Utility;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.PopulateModel;
using BIA.Entity.RequestEntity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class OrderScheduler
    {
        private readonly OrderApiCall _orderApiCall;

        public OrderScheduler(OrderApiCall orderApiCall)
        {
            _orderApiCall = orderApiCall;
        }
        public async Task BssServiceProcess(OrderDataModel item)
        {
            OrderPopulateModel pltApiObj = new OrderPopulateModel();
            MethodUrl methodUrlObj = new MethodUrl();
            int CreateCustomerRetry = 0;

            try
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                CreateCustomerRetry = Convert.ToInt16(configuration.GetSection("AppSettings:CreateCustomerRetry").Value);
            }
            catch { }

            string methodUrl = methodUrlObj.GetMethodUrl(item);

            if (item.status == (int)StatusNo.order_request)
            {
                try
                {
                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                    {
                        if (item.is_paired != null)
                        {
                            if (item.is_paired == 0)
                            {
                                // Un-Paired new registration Order area
                                NewRegUnPairReqModel orderModel = pltApiObj.PopulateNewRegUnPairOrderReq(item);
                                await _orderApiCall.PostOrderRequestToBss(item, orderModel, methodUrl);
                            }
                            else if (item.is_paired == 1)
                            {
                                // Paired new registration Order area
                                NewRegPairReqModel orderModel = pltApiObj.PopulateNewRegPairOrderReq(item);
                                await _orderApiCall.PostOrderRequestToBss(item, orderModel, methodUrl);
                            }
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement)
                    {
                        if (string.IsNullOrEmpty(item.poc_number))
                        {
                            // Individual Sim Replacement Order area
                            IndiSimReplceReqModel orderModel = pltApiObj.PopulateIndiSimReplceOrderReq(item);
                            await _orderApiCall.PatchOrderRequestToBss(item, orderModel, methodUrl);
                        }
                        else
                        {
                            // Corporate Sim Replacement Order area
                            CorpSimReplceReqModel orderModel = pltApiObj.PopulateCorpSimReplceOrderReq(item);
                            await _orderApiCall.PatchOrderRequestToBss(item, orderModel, methodUrl);
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.DeRegistration)//Biometric Cancellation
                    {
                        // Biometric Cancellation Order area
                        BioCancellReqModel reqModel = pltApiObj.PopulateBioCancellOrderReq(item);
                        await _orderApiCall.PatchOrderRequestToBss(item, reqModel, methodUrl);
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPRegistration)
                    {
                        // MNP Port in Order area
                        MnpPortInReqModel reqModel = pltApiObj.PopulateMnpPortInOrderReq(item);
                        await _orderApiCall.PostOrderRequestToBss(item, reqModel, methodUrl);
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        // Emergency retur Order area
                        MnpEmergReturnReqModel reqModel = pltApiObj.PopulateMnpEmergReturnOrderReq(item);
                        await _orderApiCall.PostOrderRequestToBss(item, reqModel, methodUrl);
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.mnp_port_in_cancel)
                    {
                        //MNP Posrt In Order area
                        MnpPortInCancellReqModel reqModel = pltApiObj.PopulateMnpPortInCancellOrderReq(item);
                        await _orderApiCall.PatchOrderRequestToBss(item, reqModel, methodUrl);
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        IndiSimTransferCustomerCreateReqModel customerCreateModel = pltApiObj.PopulateIndiSimTransferCustomerCreateReqModel(item);

                        string ownerCustomerId = await _orderApiCall.PostCreatCustomerRequestToBss(item, customerCreateModel, CreateCustomerRetry);

                        if (!string.IsNullOrEmpty(ownerCustomerId))
                        {
                            pltApiObj = new OrderPopulateModel();

                            IndiSimTransferReqModel orderModel = pltApiObj.PopulateIndiSimTransferReq(item, ownerCustomerId);

                            await _orderApiCall.PatchOrderRequestToBss(item, orderModel, methodUrl);
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMCategoryMigration)
                    {
                        pltApiObj = new OrderPopulateModel();
                        if (String.IsNullOrEmpty(item.package_code))
                        {
                            SimcategoryMigrationReqModelWithoutPackage orderModel = pltApiObj.PopulateSimCategoryMigrationWithoutPackageOrderReq(item);
                            await _orderApiCall.PatchOrderRequestToBss(item, orderModel, methodUrl);
                        }
                        else
                        {
                            SimcategoryMigrationReqModel orderModel = pltApiObj.PopulateSimCategoryMigrationOrderReq(item);
                            await _orderApiCall.PatchOrderRequestToBss(item, orderModel, methodUrl);
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
