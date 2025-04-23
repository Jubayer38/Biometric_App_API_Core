using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.PopulateModel
{
    public class BiometricPopulateModel
    {

        #region Biometric Varification Request Model Populate Area

        #region TOS/SIM Transfer NID to NID
        public SimTransferNidToNidBioReqModel PopulateSimTransferNidToNidBioReqModel(BiomerticDataModel item)
        {
            SimTransferNidToNidBioReqModel simTransferNidToNidBioReqModel = new SimTransferNidToNidBioReqModel();

            simTransferNidToNidBioReqModel.data = new SimTransferNidToNidBioData
            {
                id = 1,
                type = "biometric-request",

                attributes = new SimTransferNidToNidBioAttributes
                {
                    purpose_no = item.purpose_number,
                    dest_imsi = item.dest_imsi,
                    dest_doc_type_no = Convert.ToInt16(item.dest_doc_type_no),
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    src_ec_verification_required = item.src_ec_verification_required,
                    dest_sim_category = item.sim_category.ToString(),
                    src_sim_category = item.src_sim_category.ToString(),
                    dest_dob = item.dest_dob,
                    src_doc_type_no = item.src_doc_type_no,
                    src_doc_id = item.src_doc_id,
                    src_dob = item.src_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    src_left_thumb = Convert.ToBase64String(item.src_left_thumb),
                    src_left_index = Convert.ToBase64String(item.src_left_index),
                    src_right_thumb = Convert.ToBase64String(item.src_right_thumb),
                    src_right_index = Convert.ToBase64String(item.src_right_index),
                    is_b2b = false
                }
            };

            return simTransferNidToNidBioReqModel;
        }
        #endregion
        #region Individual
        public NewRegBioReqModel PopulateNewRegReqModel(BiomerticDataModel item)
        {
            NewRegBioReqModel newRegReqModel = new NewRegBioReqModel();
            newRegReqModel.data = (new NewRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new NewRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_imsi = item.dest_imsi,
                    dest_foreign_flag = item.dest_foreign_flag,
                    //dest_id_type_exp_time = item.dest_id_type_exp_time,
                    dest_sim_category = item.sim_category.ToString(),
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return newRegReqModel;
        }

        public MnpRegReqModel PopulateMnpRegReqModel(BiomerticDataModel item)
        {
            MnpRegReqModel mnpRegReqModel = new MnpRegReqModel();
            mnpRegReqModel.data = (new MnpRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new MnpRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_imsi = item.dest_imsi,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_sim_category = item.sim_category.ToString(),
                    dest_foreign_flag = item.dest_foreign_flag,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return mnpRegReqModel;
        }

        public MnpEmgRtnReqModel PopulateMnpEmgRtnRegReqModel(BiomerticDataModel item)
        {
            MnpEmgRtnReqModel mnpEmgRtnReqModel = new MnpEmgRtnReqModel();
            mnpEmgRtnReqModel.data = (new MnpEmgRtnRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new MnpEmgRtnRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_imsi = item.dest_imsi,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_sim_category = item.sim_category.ToString(),
                    dest_foreign_flag = item.dest_foreign_flag,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return mnpEmgRtnReqModel;
        }

        public DeRegBioReqModel PopulateDeRegReqModel(BiomerticDataModel item)
        {
            DeRegBioReqModel deRegReqModel = new DeRegBioReqModel();
            deRegReqModel.data = (new DeRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new DeRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_sim_category = item.sim_category.ToString(),
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return deRegReqModel;
        }

        public PortInCnlRegReqModel PopulatePortCnlRegReqModel(BiomerticDataModel item)
        {

            PortInCnlRegReqModel portInCnlRegReqModel = new PortInCnlRegReqModel();
            portInCnlRegReqModel.data = (new PortInCnlRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new PortInCnlRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    //user_name = item.user_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return portInCnlRegReqModel;
        }

        public SimRepReqModel PopulateSimRepRegReqModel(BiomerticDataModel item)
        {
            SimRepReqModel simRepReqModel = new SimRepReqModel();
            simRepReqModel.data = (new SimRepRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new SimTRepRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_imsi = item.dest_imsi,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    //dest_sim_category = item.sim_category.ToString(),
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false
                }
            });


            return simRepReqModel;
        }
        #endregion
        #region Corporate 
        public CorpSimReplacebyPocReqModel PopulateCorpSimReplacebyPocReqModel(BiomerticDataModel item)
        {
            CorpSimReplacebyPocReqModel corpSimReplacebyPocReqModel = new CorpSimReplacebyPocReqModel();
            corpSimReplacebyPocReqModel.data = (new CorpSimReplacebyPocData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpSimReplacebyPocAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_imsi = item.dest_imsi,
                    corp_sim_replace_type = item.sim_replacement_type.ToString(),
                    is_b2b = true,
                    user_name = item.user_id
                }
            });


            return corpSimReplacebyPocReqModel;
        }

        public CorpSimReplacebyAuthPerReqModel PopulateCorpSimReplacebyAuthPerReqModel(BiomerticDataModel item)
        {
            CorpSimReplacebyAuthPerReqModel corpSimReplacebyAuthPer = new CorpSimReplacebyAuthPerReqModel();
            corpSimReplacebyAuthPer.data = (new CorpSimReplacebyAuthPerData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpSimReplacebyAuthPerAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = Convert.ToInt32(item.dest_doc_type_no),
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_imsi = item.dest_imsi,
                    corp_sim_replace_type = item.sim_replacement_type.ToString(),
                    is_b2b = true,
                    user_name = item.user_id,
                    poc_doc_type_no = Convert.ToInt32(item.src_doc_type_no),
                    poc_doc_id = item.src_doc_id,
                    poc_dob = item.src_dob
                }
            });


            return corpSimReplacebyAuthPer;
        }
        #endregion
        #region POC Varification
        public CorpNewRegReqModel PopulateCorpNewRegReqModel(BiomerticDataModel item)
        {
            CorpNewRegReqModel corpNewRegReqModel = new CorpNewRegReqModel();
            corpNewRegReqModel.data = (new CorpNewRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpNewRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm")

                }
            });


            return corpNewRegReqModel;
        }

        public CorpSimReplacebyBulkReqModel PopulateCorpSimReplacebyBulkReqModel(BiomerticDataModel item)
        {

            CorpSimReplacebyBulkReqModel corpSimReplacebyBulkReqModel = new CorpSimReplacebyBulkReqModel();
            corpSimReplacebyBulkReqModel.data = (new CorpSimReplacebyBulkData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpSimReplacebyBulkAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_dob = item.dest_dob,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    //dest_imsi = item.sim_number,
                    corp_sim_replace_type = item.sim_replacement_type.ToString(),
                    is_b2b = true,
                    user_name = item.user_id,
                    dest_ec_verification_required = item.dest_ec_verification_required
                }
            });


            return corpSimReplacebyBulkReqModel;
        }

        public CorpMnpPortInReqModel PopulateCorpMnpPortInReqModel(BiomerticDataModel item)
        {

            CorpMnpPortInReqModel corpMnpPortInReqModel = new CorpMnpPortInReqModel();
            corpMnpPortInReqModel.data = (new CorpMnpPortInData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpMnpPortInAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm")
                }
            });
            return corpMnpPortInReqModel;
        }

        public CorpMnpEmerReturnReqModel PopulateCorpMnpEmerReturnReqModel(BiomerticDataModel item)
        {

            CorpMnpEmerReturnReqModel corpMnpEmerReturnReqModel = new CorpMnpEmerReturnReqModel();
            corpMnpEmerReturnReqModel.data = (new CorpMnpEmerReturnData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpMnpEmerReturnAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true
                }
            });
            return corpMnpEmerReturnReqModel;
        }

        public CorpDeRegReqModel PopulateCorpDeRegReqModel(BiomerticDataModel item)
        {

            CorpDeRegReqModel corpDeRegReqModel = new CorpDeRegReqModel();
            corpDeRegReqModel.data = (new CorpDeRegData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpDeRegAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true
                }
            });
            return corpDeRegReqModel;
        }

        public CorpCategoryMigrationReqModel PopulateCorpCategoryMigrationReqModel(BiomerticDataModel item)
        {
            CorpCategoryMigrationReqModel corpNCategoryMigrationReqModel = new CorpCategoryMigrationReqModel();
            corpNCategoryMigrationReqModel.data = (new CorpCategoryMigrationData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpCategoryMigrationAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm")

                }
            });


            return corpNCategoryMigrationReqModel;
        }

        public CorpSimTransferBioReqModel PopulateCorpSimTransferBioReqModel(BiomerticDataModel item)
        {
            CorpSimTransferBioReqModel corpSimTransferBioReqModel = new CorpSimTransferBioReqModel();
            corpSimTransferBioReqModel.data = (new CorpSimTransferData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpSimTransferAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = true
                }
            });


            return corpSimTransferBioReqModel;
        }

        public CorpSimTransferWithOtpBioReqModel PopulateCorpSimTransferWithOTPBioReqModel(BiomerticDataModel item)
        {
            CorpSimTransferWithOtpBioReqModel corpSimTransferBioReqModel = new CorpSimTransferWithOtpBioReqModel();
            corpSimTransferBioReqModel.data = (new CorpSimTransferWithOtpData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new CorpSimTransferWithOtpAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    src_doc_type_no = item.src_doc_type_no,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    src_doc_id = item.src_doc_id,
                    src_dob = item.src_dob,
                    src_ec_verification_required = item.src_ec_verification_required,
                    is_b2b = true
                }
            });


            return corpSimTransferBioReqModel;
        }
        #endregion
        #region Two Pary Validation
        public SimTransferBioReqModel PopulateSimTransferBioReqModel(BiomerticDataModel item)
        {

            SimTransferBioReqModel simTransferBioReqModel = new SimTransferBioReqModel();
            simTransferBioReqModel.data = (new SimTransferBioData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new SimTransferBioAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    src_doc_type_no = item.src_doc_type_no,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    src_doc_id = item.src_doc_id,
                    src_dob = item.src_dob,
                    src_ec_verification_required = item.src_ec_verification_required,
                    src_left_thumb = Convert.ToBase64String(item.src_left_thumb),
                    src_left_index = Convert.ToBase64String(item.src_left_index),
                    src_right_thumb = Convert.ToBase64String(item.src_right_thumb),
                    src_right_index = Convert.ToBase64String(item.src_right_index),
                    is_b2b = true
                }
            });
            return simTransferBioReqModel;
        }

        #region EcValidationwithoutSrcFinger
        public SimTransferWithoutSrcBioReqModel PopulateSimTransferWithoutSrcBioReqModel(BiomerticDataModel item)
        {

            SimTransferWithoutSrcBioReqModel simTransferBioReqModel = new SimTransferWithoutSrcBioReqModel();
            simTransferBioReqModel.data = (new SimTransferWithoutSrcBioData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new SimTransferWithoutSrcBioAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    msisdn = item.msisdn,
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    src_doc_type_no = item.src_doc_type_no,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    src_doc_id = item.src_doc_id,
                    src_dob = item.src_dob,
                    src_ec_verification_required = item.src_ec_verification_required,
                    is_b2b = true

                }
            });
            return simTransferBioReqModel;
        }

        #endregion

        #endregion

        #region Other Porpose Modal Area

        public MSISDNReservation PopulateMSISDNReservationReqModel(string msisdn)
        {
            MSISDNReservation mSISDNReservation = new MSISDNReservation();
            mSISDNReservation.data = new MSISDN()
            {
                msisdn = msisdn
            };
            return mSISDNReservation;
        }

        public CDTRequestModel PopulateCDTRequestModel(BiomerticDataModel reqModel)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            CDTRequestModel cdtRequestModel = new CDTRequestModel();
            cdtRequestModel.data = new CDTData()
            {

                type = "residential-credit-decision-requests",
                attributes = new CDTAttributes()
                {
                    orderchannel = "RESELLER",
                    orderer = new CDTOrderer()
                    {
                        firstname = reqModel.dest_doc_id
                    ,
                        lastname = reqModel.dest_doc_id
                    ,
                        nationality = "BD"
                    ,
                        employmenttype = "NOT_GIVEN"
                    ,
                        dateofbirth = reqModel.dest_dob
                    ,
                        iddocumenttype = reqModel.dest_doc_type_no
                    ,
                        iddocumentnumber = reqModel.dest_doc_id
                    ,
                        homephonenumber = reqModel.msisdn
                    },
                    order = new CDTOrder()
                    {
                        id = secondsSinceEpoch + "_" + reqModel.dest_doc_id
                    ,
                        createdat = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                    }
                }

            };

            return cdtRequestModel;
        }

        public DbUpdModel PopulateUpdReqModelForBio(BiomerticDataModel reqModel, BioResModel resModel)
        {
            DbUpdModel updModel = new DbUpdModel();

            updModel.bss_req_id = resModel.data.request_id;
            updModel.bi_token_number = reqModel.bi_token_number;
            updModel.status = (int)EnumRAOrderStatus.BioVerificationSubmitted;
            updModel.uddate_date = DateTime.Now;
            return updModel;
        }

        #endregion 

        #region Pre to post Migration 
        public PreToPostMigrationReqModel PopulatePreToPostMigrationReqModel(BiomerticDataModel item)
        {
            PreToPostMigrationReqModel corpSimReplacebyPocReqModel = new PreToPostMigrationReqModel();
            corpSimReplacebyPocReqModel.data = (new PreToPostMigrationData()
            {
                id = 1,
                type = "biometric-request",
                attributes = new PretoPostMigrationAttributes()
                {
                    purpose_no = item.purpose_number,
                    dest_doc_type_no = item.dest_doc_type_no,
                    dest_doc_id = item.dest_doc_id,
                    user_name = item.user_id,
                    msisdn = item.msisdn,
                    reg_date = Convert.ToDateTime(item.create_date).ToString("yyyy-MM-dd HH:mm"),
                    dest_ec_verification_required = item.dest_ec_verification_required,
                    dest_sim_category = item.sim_category.ToString(),
                    dest_dob = item.dest_dob,
                    dest_left_thumb = Convert.ToBase64String(item.dest_left_thumb),
                    dest_left_index = Convert.ToBase64String(item.dest_left_index),
                    dest_right_thumb = Convert.ToBase64String(item.dest_right_thumb),
                    dest_right_index = Convert.ToBase64String(item.dest_right_index),
                    is_b2b = false,

                }
            });


            return corpSimReplacebyPocReqModel;
        }
        #endregion
        #endregion

    }
}
