# matevoice (Polinscriptor)

### Progetto per la conversione in formato testuale di file audio

------------

## Installazione

- **Solo installazione**:

  - Usa il file di installazione "Setup.msi" che puoi richiedere a Mattia (mattia.ducci93@gmail.com) o a chiunque abbia seguito le procedure del punto sotto

- **Dal progetto** (apribile SOLO su sistema operativo Windows):

  - scaricare il progetto
  - entrare nella cartella del progetto e aprire `Polinscriptor.sln` con Visual Studio.
  - Nella solution ci sono 2 progetti: `Polinscriptor` e `Setup`
  - Click col tasto destro del mouse su `Setup` -> Installa
  - Seguire la procedura di installazione e al termine della stessa sul desktop sarà apparsa un'icona "Shortcut to Polinscriptor".

- **Esportare il file di installazione**

  - aprire la solution come al punto sopra
  - Click col tasto destro del mouse sul progetto `Setup` e premere "Ricompila".
  - Andare nella cartella del progetto dove c'è il file `Polinscriptor.sln` (da esplora risorse di windows)
    - Nella cartella Setup/Release c'è un file chiamato `Setup.msi`. Quel file è portabile anche su un altra macchina per poter installare Polinscriptor

- ### Questo progetto usa .NET Core 3.1 che non viene rilasciato con l'installer del software.

  - Durante il processo di installazione nel caso in cui sul computer di destinazione non sia installato, verrà chiesto all'utente di installarlo nel suo sistema. Acconsentire.

## Come usarlo

- All'avvio il software chiederà uno username e una password. La registrazione è fatta in modo manuale dagli amministratori. Contattare il laboratorio Polin per avere un'utenza nel sistema.
- se l'autenticazione sarà andata a buon fine si aprirà una schermata nella quale sono spiegati i passi da fare per convertire un file audio nell'equivalente trascrizione testuale.