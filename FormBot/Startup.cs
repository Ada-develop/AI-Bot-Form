// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using FormBot.Bots;
using Microsoft.Bot.Configuration;
using FormBot.Services;
using Microsoft.Bot.Builder.Azure;
using FormBot.Dialogs;
using Microsoft.BotBuilderSamples;

namespace FormBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //Configure Services

            services.AddSingleton<BotServices>();


            //Configure state

            ConfigureState(services);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();


            ConfigureDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<ConnectorDialog>>();
        }


        public void ConfigureState(IServiceCollection services)
        {
            // Put connection string to Account, blob storagename to the Container
            var storageAccount = "DefaultEndpointsProtocol=https;AccountName=bgteam;AccountKey=3wNytBdHdtwCAb1fVMmg2Ev7lmqz4rWnuQiEDToR1d2pUvGF5CceXH1KP1uklZz9gEo5vqe4YoxpKAPW11Zm6A==;EndpointSuffix=core.windows.net";
            var storageContainer = "bgteam";
            services.AddSingleton<IStorage>(new AzureBlobStorage(storageAccount, storageContainer));

            //Storage for state's
            //services.AddSingleton<IStorage, MemoryStorage>();
             
            
            //Create User state
            services.AddSingleton<UserState>();
            //Create Conv state
            services.AddSingleton<ConversationState>();
            //Create an instanc of the state service
            services.AddSingleton<BotStateService>();



        }

        public void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<ConnectorDialog>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
