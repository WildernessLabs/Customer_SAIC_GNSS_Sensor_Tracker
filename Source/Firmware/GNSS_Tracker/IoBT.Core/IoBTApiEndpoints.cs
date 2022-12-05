using System.Threading.Tasks;
using IoBT.Core.ApiModels;
using Refit;

namespace IoBT.Core
{
    public interface IobtApi
    {
        [Post("/ClientHub/Position")]
        Task SetPosition(SPEC_Position pos);

        [Post("/ClientHub/Environment")]
        Task SetEnvironment(SPEC_Environment env);
    }
}