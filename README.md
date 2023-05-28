## 📩Download Service
Questo progetto verrà utilizzato per scaricare i video e mettere nella cartella.
### Information general:
> Notes: `require` volume mounted on Docker and se hai abilitato il proxy, nella cartella proxy devi inserire gli indirizzi ip con il comma, come in questo esempio: `http:1111:1234,http:2222:1234`

### Dependencies
| Services | Required |
| ------ | ------ |
| Api | ✅  |
| RabbitMQ | ✅  |

### Variabili globali richiesti:
```sh
example:
    #--- rabbit ---
    USERNAME_RABBIT: "guest" #guest [default]
    PASSWORD_RABBIT: "guest" #guest [default]
    ADDRESS_RABBIT: "localhost" #localhost [default]
    LIMIT_CONSUMER_RABBIT: "5" #3 [default]
    
    #--- API ---
    ADDRESS_API: "localhost" #localhost [default]
    PORT_API: "33333" #3000 [default]
    PROTOCOL_API: "http" or "https" #http [default]
    
    #--- Logger ---
    LOG_LEVEL: "Debug|Info|Error" #Info [default]
    WEBHOOK_DISCORD_DEBUG: "url" [not require]
    
    #--- proxy ---
    PROXY_ENABLE: "true"

    #--- General ---
    DELAY_RETRY_ERROR: "60000" #10000 [default]
    MAX_DELAY: "40" #5 [default]
    LIMIT_THREAD_PARALLEL: "100" #5 [default]
    PATH_TEMP: "/tmp/folder" #D:\\TestAnime\\temp [default]
    BASE_PATH: "/folder/anime" or "D:\\\\Directory\Anime" #/ [default]
```