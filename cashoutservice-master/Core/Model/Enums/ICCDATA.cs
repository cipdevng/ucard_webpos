using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Enums {
    public enum ICCDATA {
        APPLICATION_INTERCHANGE_PROFILE = 0x82,
        ATC = 0x9F36,
        CID = 0x9F27,
        CVM_RESULT = 0x9F34,
        IAD = 0x9F10,
        TERMINAL_CAPABILITIES= 0x9F33,
        DF_NAME= 0x84,
        TERMINAL_TYPE= 0x9F35,
        UNPREDICTABLE_NUMBER = 0x9F37,
        AMOUNT = 0x9F03,
        AMOUNT_AUTHORISED = 0x9F02,
        PAN_SN = 0x5F34,
        TCC = 0x9F1A,
        T_CURRECNCY_CODE = 0x5F2A,
        TRANSACTION_TYPE = 0x9C,
        TRANSACTION_DATE = 0x9A,
        APPLICATION_CRYPTOGRAM = 0x9F26,
        TRANSACTION_SEQUENCE_COUNTER = 0x9F41,
        TERMINAL_VERIFICATION_RESULT = 0x95,
        IFD_SN = 0x9F1E,
    }
}
