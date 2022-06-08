using CommonEntityModel.EntityModel;
using _3DPOSRegistrationApp.Utility.UserModel_View;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DPOSRegistrationApp.Utility.ServiceModel
{
    public class ModelMapper : Profile
    {
        public ModelMapper()
        {
            CreateMap<ApplicationUser, RegistrationViewModel>();
            CreateMap<RegistrationViewModel, ApplicationUser>();
        }

    }
}
