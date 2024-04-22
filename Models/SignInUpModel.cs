using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.Models
{
    public class SignInUpModel
    {
        public SignInModel SignInModel { get; set; }
        public SignUpUserModel SignUpModel { get; set; }

        public SignInUpModel()
        {
            SignInModel = new SignInModel();
            SignUpModel = new SignUpUserModel();
        }
    }
}
