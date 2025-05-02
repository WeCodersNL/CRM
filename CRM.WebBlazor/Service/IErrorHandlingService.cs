using CRM.Model.ApplicationModels;

namespace CRM.WebBlazor.Service
{
    public interface IErrorHandlingService
    {
        Task<ResponseModel<T>> HandleErrorResponse<T>(HttpResponseMessage response);
        ResponseModel<T> HandleErrorResponse<T>(ResponseModel<T> response);
    }
}
