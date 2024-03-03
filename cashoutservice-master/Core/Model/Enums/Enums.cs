using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Enums {    
    public enum AdminAccountType {
        ADMIN,
        ORGANIZATION
    }
    public enum AccountStatus {
        ACTIVE = 1,
        ARCHIVED = -1,
        DELETED = -2,
        PENDING_ACTIVATION = 0
    }
    public enum AccountType {
        SAVINGS = 10,
        CURRENT = 20,
        CREDIT = 30,
        DEFAULT = 0,
        UNIVERSAL_ACCOUNT = 40,
        INVESTMENT_ACCOUNT = 50
    }

    public enum Tags {
        CHANNEL_SERIAL = 1,
        CTMS_DATE_AND_TIME = 2,
        CAICode = 3,
        TIMEOUT = 4,
        CURRENCY_CODE = 5,
        COUNTRY_CODE = 6,
        CALL_HOME_TIME = 7,
        MERCHANT_NAME_AND_LOCATION = 52,
        MERCHANT_CATEGORY_CODE = 8
    }
    public enum TagLength {
        CHANNEL_SERIAL = 0,
        CTMS_DATE_AND_TIME = 14,
        CAICode = 15,
        TIMEOUT = 2,
        CURRENCY_CODE = 3,
        COUNTRY_CODE = 3,
        CALL_HOME_TIME = 2,
        MERCHANT_NAME_AND_LOCATION = 40,
        MERCHANT_CATEGORY_CODE = 4
    }
}
