using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Mvc;
using GlobalUtility;
using static DLMS_SERVER.Controllers.PeopleController;

namespace DLMS_SERVER.ValidatorClasses
{
    public static class ContactValidator
    {


        public static  ValidateResult  ValidateContacts(string phone, string email, bool RequireFullConatctsForNewPerson = false)
        {
            if (RequireFullConatctsForNewPerson) 
            {
                if (string.IsNullOrEmpty(phone) || string.IsNullOrWhiteSpace(phone)) 
                {
                    return ValidateResult.Fail(415," لم يتم إرسال أي بيانات تخص رقم الهاتف.");
                }

                if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email)) 
                {
                    return ValidateResult.Fail(415," لم يتم إرسال أي بيانات تخص البريد الإلكتروني");
                }
            }
            if (
                (string.IsNullOrEmpty(phone) || string.IsNullOrWhiteSpace(phone))
                &&
                (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))

                )
            {
                return ValidateResult.Fail(415," لم يتم إرسال أي بيانات تخص رقم الهاتف أو البريد الإلكتروني");
            }

            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrWhiteSpace(phone))
            {
                if (!Validator.IsPhoneNumberValid(phone))
                    return ValidateResult.Fail(409, "رقم الهاتف الذي ادخلته غير صالح");

                if (Phone.IsExist(phone))
                    return ValidateResult.Fail(409, "رقم الهاتف موجود مسبقا في النظام يرجى إدخال رقم هاتف أخر");
            }
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrWhiteSpace(email))
            {
                if (!Validator.IsEmailAddressValid(email))
                    return ValidateResult.Fail(409, "البريد الإلكتروني الذي أدخلته ليس بريدا إلكترونيا صالحا");

                if (EmailAddress.IsExist(email))
                    return ValidateResult.Fail(409, " هذا البريد مستخدم في النظام يرجى إدخال بريد إلكتروني أخر");
            }

            return ValidateResult.Success();
        }
    }
}
