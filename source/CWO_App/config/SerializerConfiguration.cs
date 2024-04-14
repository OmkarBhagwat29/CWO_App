using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CWO_App.config
{
    public static class SerializerConfiguration
    {
        /// <summary>
        ///    Add JsonSerializerOptions, see example <see cref="ModalModule.ViewModels.ModalModuleViewModel"/>
        /// </summary>
        public static void AddSerializerOptions(this IServiceCollection services)
        {
            services.Configure<JsonSerializerOptions>(options =>
            {
                options.WriteIndented = true;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.Converters.Add(new JsonStringEnumConverter());
            });
        }
    }
}
