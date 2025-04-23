using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.PopulateModel
{
    public class OrderPopulateModel
    {
        DbUpdModel updModel;
        #region Order Request Model Populate Area
        public NewRegPairReqModel PopulateNewRegPairOrderReq(OrderDataModel item)
        {
            NewRegPairReqModel newRegPairReqModel = new NewRegPairReqModel();

            newRegPairReqModel.data = (new NewRegPairData()
            {
                type = "orders",
                attributes = new NewRegPairAttributes()
                {
                    offer = "C_ACQ_PRE_POST_ACC_ALL",
                    brand = 0,
                    delivery_type = "direct",
                    correction_for = "",
                    ordered_at = item.create_date.ToString(),
                    biometric_request_id = item.bss_request_id

                }
            });

            newRegPairReqModel.meta = (new NewRegPairMeta()
            {
                customer = new NewRegPairCustomer()
                {
                    province = "Dhaka",//item.thana_Name,
                    post_code = "unknown",//item.postal_code ?? "",
                    area = "unknown",//item.village,
                    //id_expiry = item.dest_id_type_exp_time ?? "",
                    alt_contact_phone = "unknown",//item.alt_msisdn ?? "",
                    road = "unknown",//item.road_number ?? "",
                    city = "unknown",//item.district_Name,
                    house_number = "unknown",//item.house_number ?? "",
                    co_address = "unknown",//"",
                    street = "unknown",//item.flat_number ?? "",
                    last_name = "unknown",//"",
                    language = "en",
                    title = "notitle",//item.gender,
                    is_company = false,//retailer alaways send b2c order
                    country = "BD",
                    marketing_own = true,//false,
                    id_type = item.dest_doc_type_no,//--Red
                    id_number = item.dest_doc_id,
                    birthday = item.dest_dob,
                    contact_phone = "unknown",//item.msisdn,
                    nationality = "BD",
                    postal_code = "Dhaka",//item.division_Name,
                    invoice_delivery_method = "delivery_not_needed",//item.sim_category == 1 ? "sms" : "email",// prepaid -SMS , postpaid -email
                    first_name = "unknown",//item.customer_name,
                    email = "unknown@gmail.com",//item.email ?? "",
                    occupation = "unknown"//""
                },

                sales_info = new NewRegPairSales_Info()
                {
                    reseller = item.center_or_distributor_code, // reseller user name-    ///CenterCode/Dristirbutor Code 
                    salesman = item.salesman_code, // reseller user name-RetailerCode     /// User Name
                    channel = item.channel_name,
                    chain = "",
                    sales_type = "acquisition",
                    msisdn = item.msisdn
                },

                products = new System.Collections.ArrayList()
                {

                    new NewRegPairProduct()
                    {
                        barrings= new object[0],
                        termination_penalty_fee = 0,
                        connection_type = "4G",//low level doc
                        //payer = "owner",
                        initial_period = 24,
                        user_privacy = "none",
                        msisdn = item.msisdn,
                        paying_monthly = true,// for prepaid false else true
                        recurring_period = 24,
                        retention_penalty_fee=0,
                        type = item.subscription_code,// -yellow
                        product_type = "Subscription",
                        payer = new NewRegPairPayer()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        //user = "payer",
                        user = new NewRegPairUser()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        packages = new object()
                    },
                     new NewRegPairProduct1()
                    {
                        type = "USIM",//(Re-Confram needed from DBSS- Static now)  //low level doc--red -we will get this value from valided sim api, which is not ready.
                        article_id=item.sim_number,//chack if contain 898802 else concat this in starting
                        data_dict =new NewRegPairData_Dict()
                        {
                            msisdn=item.msisdn
                        },
                         price = 0,//low level doc
                         product_type = "Simcard"
                    }


                }
            });
            return newRegPairReqModel;
        }

        public NewRegUnPairReqModel PopulateNewRegUnPairOrderReq(OrderDataModel item)
        {
            dynamic package = new ExpandoObject();
            IDictionary<string, object> pack = package;
            pack.Add(item.package_code, true);
            NewRegUnPairReqModel newRegUnPairReqModel = new NewRegUnPairReqModel();

            newRegUnPairReqModel.data = (new NewRegUnPairData()
            {
                type = "orders",
                attributes = new NewRegUnPairAttributes()
                {
                    offer = "C_ACQ_PRE_POST_ACC_ALL",
                    brand = 0,
                    delivery_type = "direct",
                    correction_for = "",
                    ordered_at = item.create_date.ToString(),
                    biometric_request_id = item.bss_request_id

                }
            });

            newRegUnPairReqModel.meta = (new NewRegUnPairMeta()
            {
                customer = new NewRegUnPairCustomer()
                {
                    province = "Dhaka",//item.thana_Name,
                    post_code = "unknown",//item.postal_code ?? "",
                    area = "unknown",//item.village,
                    //id_expiry = item.dest_id_type_exp_time ?? "",
                    alt_contact_phone = "unknown",//item.alt_msisdn ?? "",
                    road = "unknown",//item.road_number ?? "",
                    city = "unknown",//item.district_Name,
                    house_number = "unknown",//item.house_number ?? "",
                    co_address = "unknown",//"",
                    street = "unknown",//item.flat_number ?? "",
                    last_name = "unknown",//"",
                    language = "en",
                    title = "notitle",//item.gender,
                    is_company = false,//retailer alaways send b2c order
                    country = "BD",
                    marketing_own = true,
                    id_type = item.dest_doc_type_no,//--Red
                    id_number = item.dest_doc_id,
                    birthday = item.dest_dob,
                    contact_phone = "unknown",//item.msisdn,
                    nationality = "BD",
                    postal_code = "Dhaka",//item.division_Name,
                    invoice_delivery_method = "delivery_not_needed",//item.sim_category == 1 ? "sms" : "email",// prepaid -SMS , postpaid -email
                    first_name = "unknown",//item.customer_name,
                    email = "unknown@gmail.com",//item.email ?? "",
                    occupation = "unknown"//""
                },

                sales_info = new NewRegUnPairSales_Info()
                {
                    reseller = item.center_or_distributor_code, // reseller user name-    ///CenterCode/Dristirbutor Code 
                    salesman = item.salesman_code, // reseller user name-RetailerCode     /// User Name
                    channel = item.channel_name,
                    chain = "",
                    sales_type = "acquisition",
                    msisdn = item.msisdn
                },

                products = new System.Collections.ArrayList()
                {

                    new NewRegUnPairProduct()
                    {
                        barrings= new object[0],
                        termination_penalty_fee = 0,
                        connection_type = "4G",//low level doc
                        //payer = "owner",
                        initial_period = 24,
                        user_privacy = "none",
                        msisdn = item.msisdn,
                        paying_monthly = true,// for prepaid false else true
                        recurring_period = 24,
                        retention_penalty_fee=0,
                        type = item.subscription_code,// -yellow
                        product_type = "Subscription",
                        payer = new NewRegUnPairPayer()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        //user = "payer",
                        user = new NewRegUnPairUser()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        packages = pack
                    },
                     new NewRegUnPairProduct1()
                    {
                        type = "USIM",//(Re-Confram needed from DBSS- Static now)  //low level doc--red -we will get this value from valided sim api, which is not ready.
                        article_id=item.sim_number,//chack if contain 898802 else concat this in starting
                        data_dict =new NewRegUnPairData_Dict()
                        {
                            msisdn=item.msisdn
                        },
                         price = 0,//low level doc
                         product_type = "Simcard"
                    }


                }
            });


            return newRegUnPairReqModel;
        }

        public MnpPortInReqModel PopulateMnpPortInOrderReq(OrderDataModel item)
        {
            dynamic package = new ExpandoObject();
            IDictionary<string, object> pack = package;
            pack.Add(item.package_code, true);

            MnpPortInReqModel mnpPortInReqModel = new MnpPortInReqModel();
            mnpPortInReqModel.data = (new MnpPortInData()
            {
                type = "orders",
                attributes = new MnpPortInAttributes()
                {
                    brand = 0,//
                    correction_for = "",//
                    delivery_type = "direct",//
                    offer = "-",//
                    biometric_request_id = item.bss_request_id
                }
            });

            mnpPortInReqModel.meta = (new MnpPortInMeta()
            {
                customer = new MnpPortInCustomer()
                {
                    alt_contact_phone = "unknown",//item.alt_msisdn ?? "",
                    area = "unknown",//item.village,
                    birthday = item.dest_dob,
                    city = "unknown",//item.district_Name,
                    co_address = "unknown",//"",
                    contact_phone = "unknown",//item.msisdn,
                    country = "BD",
                    email = "unknown@gmail.com",//item.email ?? "",
                    first_name = "unknown",//item.customer_name,
                    house_number = "unknown",//item.house_number ?? "",
                    //id_expiry = item.dest_id_type_exp_time,
                    id_number = item.dest_doc_id,
                    id_type = item.dest_doc_type_no,
                    invoice_delivery_method = "delivery_not_needed",//item.sim_category == 1 ? "sms" : "email",// prepaid -SMS , postpaid -email
                    is_company = false,
                    language = "en",
                    last_name = "unknown",//"",
                    marketing_own = true,//false,
                    occupation = "unknown",//"",
                    postal_code = "Dhaka",//item.division_Name,
                    post_code = "unknown",//item.postal_code ?? "",
                    nationality = "BD",
                    province = "Dhaka",//item.thana_Name,
                    road = "unknown",//item.road_number ?? "",
                    street = "unknown",//item.flat_number ?? "",
                    title = "notitle"//item.gender
                },

                sales_info = new MnpPortInSales_Info()
                {
                    chain = "",//
                    channel = item.channel_name,
                    msisdn = item.msisdn,
                    reseller = item.center_or_distributor_code, // reseller user name 
                    sales_type = "acquisition",
                    salesman = item.salesman_code,
                },

                products = new System.Collections.ArrayList()
                {
                    new MnpPortInProduct()
                    {
                        barrings= new object[0],
                        initial_period = 24,// from low level detection.
                         mnp = new MnpPortIn()
                            {
                                document_id = item.dest_doc_id,
                                msisdn = item.msisdn,
                                portation_time = item.port_in_date,
                                recipient_operator = "Banglalink",//filed not available in our database
                                is_emergency_return = false

                            },
                        msisdn = item.msisdn,
                        packages=pack,
                        //payer = "owner",// from low level detection.
                        paying_monthly = true,// from low level detection.
                        product_type = "Subscription",// from low level detection.
                        payer = new MnpPortInPayer()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        recurring_period = 24,// from low level detection.
                        retention_penalty_fee=0,// from low level detection.
                        termination_penalty_fee=0,// from low level detection.
                        type = item.subscription_code,
                        //user = "payer",// from low level detection.
                        user = new MnpPortInUser()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        user_privacy = "none",// from low level detection.
                    },
                     new MnpPortInProduct1()
                    {
                        type = "USIM",//(Re-Confram needed from DBSS- Static now)  //low level doc--red -we will get this value from valided sim api, which is not ready.
                        article_id=item.sim_number,//chack if contain 898802 else concat this in starting
                        data_dict =new MnpPortInData_Dict()
                        {
                            msisdn=item.msisdn
                        },
                         price = 0,//low level doc
                         product_type = "Simcard"
                    }
                }
            });
            return mnpPortInReqModel;
        }

        public MnpEmergReturnReqModel PopulateMnpEmergReturnOrderReq(OrderDataModel item)
        {
            dynamic package = new ExpandoObject();
            IDictionary<string, object> pack = package;
            pack.Add(item.package_code, true);
            MnpEmergReturnReqModel mnpEmergReturnReqModel = new MnpEmergReturnReqModel();
            mnpEmergReturnReqModel.data = (new MnpEmergReturnData()
            {
                type = "orders",
                attributes = new MnpEmergReturnAttributes()
                {
                    brand = 0,
                    correction_for = "",
                    delivery_type = "direct",
                    offer = "",
                    biometric_request_id = item.bss_request_id
                }
            });

            mnpEmergReturnReqModel.meta = (new MnpEmergReturnMeta()
            {
                customer = new MnpEmergReturnCustomer()
                {
                    alt_contact_phone = "unknown",//item.alt_msisdn ?? "",
                    area = "unknown",//item.village,
                    birthday = item.dest_dob,
                    city = "unknown",//item.district_Name,
                    co_address = "unknown",//"",
                    contact_phone = "unknown",//item.msisdn,
                    country = "BD",
                    email = "unknown@gmail.com",//item.email ?? "",
                    first_name = "unknown",//item.customer_name,
                    house_number = "unknown",//item.house_number ?? "",
                    id_expiry = item.dest_id_type_exp_time,
                    id_number = item.dest_doc_id,
                    id_type = item.dest_doc_type_no,
                    invoice_delivery_method = "delivery_not_needed",//item.sim_category == 1 ? "sms" : "email",// prepaid -SMS , postpaid -email
                    is_company = false,
                    language = "en",
                    last_name = "unknown",//"",
                    marketing_own = true,//false
                    occupation = "unknown",//"",
                    postal_code = "Dhaka",//item.division_Name,
                    post_code = "unknown",//item.postal_code ?? "",
                    nationality = "BD",
                    province = "Dhaka",//item.thana_Name,
                    road = "unknown",//item.road_number ?? "",
                    street = "unknown",//item.flat_number ?? "",
                    title = "notitle"//item.gender
                },

                sales_info = new MnpEmergReturnSales_Info()
                {
                    chain = "",//
                    channel = item.channel_name,
                    //msidn = "",// msidn what it is?
                    msisdn = item.msisdn,
                    reseller = item.center_or_distributor_code, // reseller user name 
                    sales_type = "acquisition",
                    salesman = item.salesman_code,
                },

                products = new System.Collections.ArrayList()
                {
                    new MnpEmergReturnProduct()
                    {
                        barrings= new object[0],
                        initial_period = 24,// from low level detection.
                         mnp = new MnpEmergReturn()
                            {
                                 document_id = item.dest_doc_id,
                                msisdn = item.msisdn,
                                portation_time = item.port_in_date,
                                recipient_operator = "",//filed not available in our database
                                is_emergency_return = true

                            },
                        msisdn = item.msisdn,
                        packages=pack,
                        //payer = "owner",// from low level detection.
                        paying_monthly = true,// from low level detection.
                        product_type = "Subscription",// from low level detection.
                        payer = new MnpEmergReturnPayer()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        recurring_period = 24,// from low level detection.
                        retention_penalty_fee=0,// from low level detection.
                        termination_penalty_fee=0,// from low level detection.
                        type = item.subscription_code,
                        //user = "payer",// from low level detection.
                        user = new MnpEmergReturnUser()
                        {
                            province = item.thana_Name,
                            post_code = item.postal_code ?? "",
                            area = item.village,
                            alt_contact_phone = item.alt_msisdn ?? "",
                            road = item.road_number ?? "",
                            city = item.district_Name,
                            house_number = item.house_number ?? "",
                            co_address = "",
                            street = item.flat_number ?? "",
                            last_name = "",
                            language = "en",
                            title = item.gender,
                            country = "BD",
                            contact_phone = item.msisdn,
                            nationality = "BD",
                            postal_code = item.division_Name,
                            invoice_delivery_method = item.sim_category == 1 ? "sms" : "email",
                            first_name = item.customer_name,
                            email = item.email ?? "",
                            occupation = ""
                        },
                        user_privacy = "none",// from low level detection.
                    },
                     new MnpEmergReturnProduct1()
                    {
                        type = "USIM",//(Re-Confram needed from DBSS- Static now)  //low level doc--red -we will get this value from valided sim api, which is not ready.
                        article_id=item.sim_number,//chack if contain 898802 else concat this in starting
                        data_dict =new MnpEmergReturnData_Dict()
                        {
                            msisdn=item.msisdn
                        },
                         price = 0,//low level doc
                         product_type = "Simcard"
                    }

                }
            });
            return mnpEmergReturnReqModel;
        }

        public MnpPortInCancellReqModel PopulateMnpPortInCancellOrderReq(OrderDataModel item)
        {
            MnpPortInCancellReqModel mnpPortInCancellReq = new MnpPortInCancellReqModel();

            mnpPortInCancellReq.data = (new MnpPortInCancellData()
            {
                type = "order-cancellation",
                id = "1",// as 
                attributes = new MnpPortInCancellAttributes()
                {
                    id = item.port_in_confirmation_code,//Mnp Port In Order id
                    biometric_request_id = item.bss_request_id
                }
            });

            return mnpPortInCancellReq;
        }

        public IndiSimReplceReqModel PopulateIndiSimReplceOrderReq(OrderDataModel item)
        {
            IndiSimReplceReqModel indiSimReplceReqModel = new IndiSimReplceReqModel();

            indiSimReplceReqModel.data = (new IndiSimReplceData()
            {
                type = "sim-change",
                id = item.old_sim_number,//Old SIM Number
                attributes = new IndiSimReplceAttributes()
                {
                    biometric_request_id = item.bss_request_id,
                    new_icc = item.sim_number,// New SIM Number
                    reason = item.sim_replace_reason,
                    payment_mode = item.payment_type,
                    meta = new IndiSimReplceMeta()
                    {
                        channel = item.channel_name,
                        reseller = item.center_or_distributor_code,
                        salesman = item.salesman_code
                    }
                }

            });

            return indiSimReplceReqModel;
        }

        public CorpSimReplceReqModel PopulateCorpSimReplceOrderReq(OrderDataModel item)
        {
            CorpSimReplceReqModel corpSimReplceReqModel = new CorpSimReplceReqModel();

            if (!String.IsNullOrEmpty(item.payment_type))
            {
                corpSimReplceReqModel.data = (new CorpSimReplceData()
                {
                    type = "sim-change",
                    id = item.old_sim_number,//Old SIM Number
                    attributes = new CorpSimReplceAttributes()
                    {
                        biometric_request_id = item.bss_request_id,
                        new_icc = item.sim_number,// New SIM Number
                                                  //reason = string.IsNullOrEmpty(item.payment_type) ? "Others" : item.payment_type,
                        reason = "Others",
                        payment_mode = item.payment_type,//"FOC",
                        meta = new CorpSimReplceMeta()
                        {
                            channel = item.channel_name,
                            reseller = item.center_or_distributor_code,
                            salesman = item.salesman_code
                        }
                    }
                });
            }
            else
            {
                corpSimReplceReqModel.data = (new CorpSimReplceData()
                {
                    type = "sim-change",
                    id = item.old_sim_number,//Old SIM Number
                    attributes = new CorpSimReplceAttributes()
                    {
                        biometric_request_id = item.bss_request_id,
                        new_icc = item.sim_number,// New SIM Number
                                                  //reason = string.IsNullOrEmpty(item.payment_type) ? "Others" : item.payment_type,
                        reason = "Others",
                        payment_mode = "FOC",
                        meta = new CorpSimReplceMeta()
                        {
                            channel = item.channel_name,
                            reseller = item.center_or_distributor_code,
                            salesman = item.salesman_code
                        }
                    }

                });
            }

            

            return corpSimReplceReqModel;
        }

        public BioCancellReqModel PopulateBioCancellOrderReq(OrderDataModel item)
        {
            BioCancellReqModel bioCancellReqModel = new BioCancellReqModel();
            bioCancellReqModel.data = (new BioCancellData()
            {
                type = "subscriptions",
                id = item.dbss_subscription_id.ToString(),
                attributes = new BioCancellAttributes()
                {
                    biometric_request_id = item.bss_request_id,
                    status = 0
                }
            });
            bioCancellReqModel.meta = (new BioCancellMeta()
            {
                channel = item.channel_name,
                reseller = item.center_or_distributor_code,
                salesman = item.salesman_code,
                reason = "personal"
            });
            return bioCancellReqModel;
        }

        public IndiSimTransferCustomerCreateReqModel PopulateIndiSimTransferCustomerCreateReqModel(OrderDataModel item)
        {
            IndiSimTransferCustomerCreateReqModel indiSimTransferCustomerCreateReqModel = new IndiSimTransferCustomerCreateReqModel();

            string idDocumentType = "";

            if (item.dest_doc_id.Length == 10)
            {
                idDocumentType = "smart_national_id";
            }
            else
            {
                idDocumentType = "national_id";
            }

            indiSimTransferCustomerCreateReqModel.data = (new IndiSimTransferCustomerCreateData()
            {
                type = "customers",
                attributes = new IndiSimTransferCustomerCreateAttributes()
                {
                    id_document_type = idDocumentType,
                    id_document_number = item.dest_doc_id,
                    birthday = item.dest_dob,
                    nationality = "BD"
                }
            });

            return indiSimTransferCustomerCreateReqModel;
        }

        public IndiSimTransferReqModel PopulateIndiSimTransferReq(OrderDataModel item, string owner_customer_id)
        {
            IndiSimTransferReqModel indiSimTransferReqModel = new IndiSimTransferReqModel();

            indiSimTransferReqModel.data = (new IndiSimTransferReqData()
            {
                type = "subscriptions",
                id = item.dbss_subscription_id.ToString(),
                attributes = new IndiSimTransferReqAttributes()
                {
                    owner_customer = new IndiSimTransferReqOwnerCustomer()
                    {
                        id = owner_customer_id,
                        type = "customers"
                    },
                    biometric_request_id = item.bss_request_id,
                    _meta = new IndiSimTransferReqMeta()
                    {
                        channel = item.channel_name
                    }
                }

            });

            return indiSimTransferReqModel;
        }

        public SimcategoryMigrationReqModel PopulateSimCategoryMigrationOrderReq(OrderDataModel item)
        {
            SimcategoryMigrationReqModel corpSimReplceReqModel = new SimcategoryMigrationReqModel();

            corpSimReplceReqModel.data = (new SimcategoryMigrationData()
            {
                type = "subscription-types",
                id = item.subscription_code,
                biometric_request = item.bss_request_id,
                meta = new SimcategoryMigrationMeta()
                {
                    change_date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    send_sms = true,
                    channel = item.channel_name,
                    packages = new List<Packages>()
                    {
                        new Packages(){ name = item.package_code }
                    }
                }
            });
            return corpSimReplceReqModel;
        }

        public SimcategoryMigrationReqModelWithoutPackage PopulateSimCategoryMigrationWithoutPackageOrderReq(OrderDataModel item)
        {
            SimcategoryMigrationReqModelWithoutPackage corpSimReplceReqModel = new SimcategoryMigrationReqModelWithoutPackage();

            corpSimReplceReqModel.data = (new SimcategoryMigrationWithoutPackageData()
            {
                type = "subscription-types",
                id = item.subscription_code,
                biometric_request = item.bss_request_id,
                meta = new SimcategoryMigrationWithoutPackageMeta()
                {
                    change_date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    send_sms = true,
                    channel = item.channel_name,
                    packages = new string[] { }
                }
            });
            return corpSimReplceReqModel;
        }


        #endregion

        #region Other Region
        public DbUpdModel PopulateUpdReqModelForOrder(OrderDataModel item)
        {
            updModel = new DbUpdModel();

            updModel.bss_req_id = item.bss_request_id;
            updModel.bi_token_number = item.bi_token_number;
            updModel.confirmation_code = item.confirmation_code;
            updModel.status = (int)StatusNo.order_req_sending;
            return updModel;
        }
        #endregion
    }
}
