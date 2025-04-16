console.log("🆕 Middleware iniciado. Versão 1.0.1");


const express = require('express');
const { Client, LocalAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const axios = require('axios');

const API_ADEGA = 'http://localhost:5198';
const app = express();
app.use(express.json());

// Inicializa o cliente WhatsApp
const client = new Client({
    authStrategy: new LocalAuth()
});

// Exibe o QR Code
client.on('qr', qr => {
    console.clear();
    qrcode.generate(qr, { small: true });
    console.log("📲 Escaneie o QR Code no WhatsApp");
});

// Conexão estabelecida
client.on('ready', () => {
    console.log("✅ Conectado ao WhatsApp");
    console.log("❗🔴 Não feche a janela, minimize!!!");
});

// Recebe mensagens e responde com promoções
client.on('message', async msg => {
    if (!msg.body || !msg.from || msg.from.includes('@broadcast')) {
        console.log("⚠️ Mensagem ignorada.");
        return;
    }

    try {
        const telefoneLimpo = msg.from.split('@')[0];
        console.log("📩 Mensagem recebida de", telefoneLimpo);

        // 1. Envia o pedido para a API
        await axios.post(`${API_ADEGA}/whatsapp-pedido`, {
            telefone: telefoneLimpo,
            mensagem: msg.body
        });
        console.log("✅ Pedido salvo na API");

        // 2. Busca promoções ativas
        const response = await axios.get(`${API_ADEGA}/Promocao/Ativas`);
        const promocoes = response.data;

        // 3. Monta a resposta
        let resposta = "🍷 Olá! Recebemos seu pedido.\nEstamos preparando seu pedido.";

        if (promocoes.length > 0) {
            resposta += `\n\n🎯 *Nossas promoções:*`;
            resposta += promocoes.map(p => `\n🔸 *${p.titulo}*: ${p.descricao}`).join('');
        }

        // 4. Envia a resposta
        await client.sendMessage(msg.from, resposta);
        console.log("✅ Resposta enviada");

    } catch (err) {
        console.error("❌ Erro ao processar mensagem:");
        if (err.response) {
            console.error("📡 Erro na API:", err.response.status, err.response.data);
        } else if (err.message) {
            console.error("📜 Mensagem:", err.message);
        } else {
            console.error("🧩 Erro desconhecido:", err);
        }
    }
});

// Rota de teste para envio manual via Postman
app.post('/enviar-mensagem', async (req, res) => {
    const { telefone, mensagem } = req.body;

    try {
        await client.sendMessage(`${telefone}@c.us`, mensagem);
        console.log(`📨 Resposta enviada para ${telefone}: ${mensagem}`);
        res.sendStatus(200);
    } catch (err) {
        console.error("❌ Erro ao enviar mensagem manual:", err.message);
        res.status(500).send("Erro ao enviar mensagem");
    }
});

// Inicializa o cliente e o servidor Express
client.initialize();

app.listen(3000, () => {
    console.log("🌐 Middleware rodando na porta 3000");
});
