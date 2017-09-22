# FDM90
Services Install
1. Navigate to the service bin folder from the development command prompt shell
2. Install service: installutil.exe [ServceName].exe   
3. Start service: net start [ServiceName]

Service Uninstall
1. Stop service: net stop [ServiceName]
2. Uninstall service: installutil.exe /u [ServceName].exe   

Ensure user is admin on machine as application will create a directory in C: folder. This is the default value set in DB Configuration => "FileSaveLocation".

As Database contain tokens for live trading business, please only use own or predetermined login of fable when creating or deleting from social media.