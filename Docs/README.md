
📦 ADEGA - SISTEMA DE GESTÃO DE ESTOQUE (VERSÃO DE TESTE)

🖥️ COMO USAR:

1️⃣ Extraia o conteúdo do arquivo .zip para uma pasta de sua preferência (exemplo: C:\Adega)

2️⃣ Para abrir o sistema:
   - Dê dois cliques no arquivo: IniciarSistema.bat
   - O sistema abrirá automaticamente no navegador no endereço:
     👉 http://localhost:5000

   ✅ Login padrão:
   - Usuário: admin
   - Senha: 1234

📌 COMO INICIAR O SISTEMA AUTOMATICAMENTE COM O WINDOWS:

Se quiser que o sistema seja aberto automaticamente toda vez que o computador for ligado, siga os passos abaixo:

1. Crie um atalho do arquivo IniciarSistema.bat:
   - Clique com o botão direito sobre o arquivo > Enviar para > Área de trabalho (criar atalho)

2. Pressione as teclas Win + R e digite:
   shell:startup

3. Na pasta que será aberta, cole o atalho criado.

🟡 Importante:
- Esse processo fará com que o sistema seja iniciado automaticamente após o login do Windows.
- O sistema abrirá minimizado por causa do comando start /min usado no arquivo .bat.

⚠️ ATENÇÃO:
- Não exclua ou mova as pastas Data ou wwwroot.
- O arquivo adega.db, localizado dentro da pasta Data, é o banco de dados local do sistema.
- Esta versão está em modo de testes e tem validade de 30 dias.

📁 ESTRUTURA DOS ARQUIVOS:
- Adega.exe → Arquivo executável do sistema.
- Data/adega.db → Banco de dados do sistema.
- wwwroot/ → Imagens e arquivos estáticos.
- IniciarSistema.bat → Arquivo para iniciar o sistema de forma prática.

📞 SUPORTE:
Em caso de dúvidas, entre em contato:
- E-mail: joaorogodoy@gmail.com
- Telefone: (11) 97132-9882
