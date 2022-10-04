using EI.SI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ficha3_Server
{
    static class vars 
    {
        public static List<ClientHandler> clients = new List<ClientHandler>();
    }

    class Program
    {
        //Criar novamente uma constante, tal como feito do lado do cliente.
        private const int PORT = 10000;

        static void Main(string[] args)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, PORT);
            TcpListener listener = new TcpListener(endpoint);

            // Iniciar o listener; apresentação da primeira mensagem
            // na linha de comandos e inicialização do contador.
            listener.Start();
            Console.WriteLine("!SERVER READY!");
            int clientCounter = 0;

            //Criação do ciclo infinito de forma a que este esteja
            //sempre em execução até ordem em contrário
            while (true)
            {
                // Definição da variável client do tipo TcpClient
                TcpClient client = listener.AcceptTcpClient();

                clientCounter++;
                Console.WriteLine("Client {0} connected", clientCounter);

                //Definição da variável clientHandler do tipo TcpClient 
                ClientHandler clientHandler = new ClientHandler(client,clientCounter);

                vars.clients.Add(clientHandler);

                clientHandler.Handle();
            }
        }
        public static string CifrarTexto(string txt, AesCryptoServiceProvider aes)
        {
            //VARIÁVEL PARA GUARDAR O TEXTO DECIFRADO EM BYTES
            byte[] txtDecifrado = Encoding.UTF8.GetBytes(txt);
            //VARIÁVEL PARA GUARDAR O TEXTO CIFRADO EM BYTES
            byte[] txtCifrado;
            //RESERVAR ESPAÇO NA MEMÓRIA PARA COLOCAR O TEXTO E CIFRÁ-LO
            MemoryStream ms = new MemoryStream();
            //INICIALIZAR O SISTEMA DE CIFRAGEM (WRITE)
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            //CRIFRAR OS DADOS
            cs.Write(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();
            //GUARDAR OS DADOS CRIFRADO QUE ESTÃO NA MEMÓRIA
            txtCifrado = ms.ToArray();
            //CONVERTER OS BYTES PARA BASE64 (TEXTO)
            string txtCifradoB64 = Convert.ToBase64String(txtCifrado);
            //DEVOLVER OS BYTES CRIADOS EM BASE64
            return txtCifradoB64;
        }

        public static string DecifrarTexto(string txtCifradoB64, AesCryptoServiceProvider aes)
        {
            //VARIÁVEL PARA GUARDAR O TEXTO CIFRADO EM BYTES
            byte[] txtCifrado = Convert.FromBase64String(txtCifradoB64);
            //RESERVAR ESPAÇO NA MEMÓRIA PARA COLOCAR O TEXTO E CIFRÁ-LO
            MemoryStream ms = new MemoryStream(txtCifrado);
            //INICIALIZAR O SISTEMA DE CIFRAGEM (READ)
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            //VARIÁVEL PARA GUARDO O TEXTO DECIFRADO
            byte[] txtDecifrado = new byte[ms.Length];
            //VARIÁVEL PARA TER O NÚMERO DE BYTES DECIFRADOS
            int bytesLidos = 0;
            //DECIFRAR OS DADOS
            bytesLidos = cs.Read(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();
            //CONVERTER PARA TEXTO
            string textoDecifrado = Encoding.UTF8.GetString(txtDecifrado, 0, bytesLidos);
            //DEVOLVER TEXTO DECRIFRADO
            return textoDecifrado;
        }
        public static void broadcast(string mensagem)
        {
            NetworkStream networkStream;
            ProtocolSI protocolSI = new ProtocolSI();

            foreach (ClientHandler client in vars.clients)
            {
                    networkStream = client.networkStream;
                    byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, CifrarTexto(mensagem, client.aes));
                    networkStream.Write(packet, 0, packet.Length);
            }
            
        }
         public static void ficheiros(string txt)
         {
            string path = @"logs.txt";
            File.AppendAllText(path, txt + Environment.NewLine, Encoding.UTF8);
        }
    }

    class ClientHandler
    {
        private TcpClient client;
        private int clientID;
        public string mensagem ;
        public string palavraCifragem;
        public NetworkStream networkStream;
        public ProtocolSI protocolSI;
        public string mensagemDecifrada;

        //algoritmo simétrico e asimetrico
        public AesCryptoServiceProvider aes;
        public RSACryptoServiceProvider rsa;
        public ClientHandler(TcpClient client, int clientID) 
        {
            this.client = client;
            this.clientID = clientID; 
        }

        public void Handle() 
        {
            Thread thread = new Thread(ThreadHandler);
            thread.Start();
        }
        private string GerarIv(string pass)
        {
            byte[] salt = new byte[] { 7, 8, 7, 8, 2, 5, 9, 5 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);
            //GERAR UMA KEY
            byte[] iv = pwdGen.GetBytes(16);
            aes.IV = iv;
            //CONVERTER PARA BASE64
            string ivB64 = Convert.ToBase64String(iv);
            //DEVOLVER EM BYTES
            return ivB64;
        }
        private string GerarChavePrivada(string pass)
        {
            // O salt, explicado de seguida tem de ter no mínimo 8 bytes e não
            //é mais do que array be bytes. O array é caracterizado pelo []
            byte[] salt = new byte[] { 0, 1, 0, 8, 2, 9, 9, 7 };
            /* A Classe Rfc2898DeriveBytes é um método para criar uma chave e um vector de inicialização.
				Esta classe usa:
				pass = password usada para derivar a chave;
				salt = dados aleatório usado como entrada adicional. É usado para proteger password.
				1000 = número mínimo de iterações recomendadas
			*/
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);
            //GERAR KEY
            byte[] key = pwdGen.GetBytes(16);
            aes.Key = key;
            //CONVERTER A PASS PARA BASE64. A BASE64 é um método 
            //para codificação de informação e consequente transferência de dados pela Internet
            string passB64 = Convert.ToBase64String(key);
            //DEVOLVER A PASS EM BYTES
            return passB64;
        }
        private static string GenerateSalt()
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[8];
            rng.GetBytes(buff);
            return Encoding.UTF8.GetString(buff);
        }
        public void GerarChave()
        {
            palavraCifragem = GenerateSalt();
            //INICIALIZAR SERVIÇO DE CIFRAGEM AES e RSA
            aes = new AesCryptoServiceProvider();
            rsa = new RSACryptoServiceProvider();
            GerarChavePrivada(palavraCifragem);
            GerarIv(palavraCifragem);
        }
        private string encriptarRSA(string chaveCifrada)
        {
            byte[] dados = Encoding.UTF8.GetBytes(chaveCifrada);
            //CIFRAR DADOS UTILIZANDO RSA
            byte[] dadosEnc = rsa.Encrypt(dados, true);
            return Convert.ToBase64String(dadosEnc);
        }
        public void ThreadHandler() 
        {
            networkStream = this.client.GetStream();
            protocolSI = new ProtocolSI();

            //gera a chave mal inicia o servidor
            GerarChave();

            //recebe a chave publica do cliente 
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            string chavePublica= protocolSI.GetStringFromData();

            //atribui a chave publica ao RSA
            rsa.FromXmlString(chavePublica);

            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);

            //Encripta a palavraCrifragem 
            byte[] packet1 = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, encriptarRSA(palavraCifragem));
            networkStream.Write(packet1, 0, packet1.Length);

            while(protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
            // Ciclo a ser executado até ao fim da transmissão.
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT) 
            {
                try
                {
                    int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // "Alteração"/mudança entre a apresentação
                // da mensagem e o fim da tranmissão.
                switch (protocolSI.GetCmdType())
                {
                        case ProtocolSICmdType.DATA:

                        //Recebe a mensagem do cliente
                        string mensagem2 = protocolSI.GetStringFromData();

                        mensagemDecifrada = Program.DecifrarTexto(mensagem2, aes);

                        //Adiciona á mensagem o Id do cliente e a mesnsagem decifrada
                        mensagem = "Client " + clientID + ": " + mensagemDecifrada;

                        DateTime dataAtual = DateTime.Now;

                        Program.ficheiros(mensagem + " " + dataAtual);

                        Console.WriteLine("Client " + clientID + ": " + mensagem2);

                        //manda para os clientes a mensagem
                        
                        break;

                        //recebe a signature do cliente
                        case ProtocolSICmdType.USER_OPTION_3:

                        byte[] signature = protocolSI.GetData();
                        string signature2 = Convert.ToBase64String(signature);

                        bool verify;

                        using (SHA1 sha1 = SHA1.Create())
                        {
                            signature = Convert.FromBase64String(signature2);
                            byte[] dados = Encoding.UTF8.GetBytes(mensagemDecifrada);

                            // VERIFICA QUE UMA ASSINATURA DIGITIAL É VÁLIDA,
                            // ATRAVÉS DO VALOR DA HASH NA ASSINATURA.
                            // ESTA VALIDAÇÃO USA A CHAVE PÚBLICA  E COMPARA-A COM O VALOR DA HASH
                            // PROVIDENCIADA.
                            verify = rsa.VerifyData(dados, sha1, signature);
                        }
                        if(verify)
                        {
                            Program.broadcast(mensagem);
                        }
                        else
                        {
                            Console.WriteLine("Verificação da Mensagem Falhou");
                        }
                        break;

                     case ProtocolSICmdType.EOT:
                        Console.WriteLine(mensagem);
                        Console.WriteLine("Ending thread from Client {0} " + clientID);

                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                }
            }

            // Fecho do networkStream e do cliente (TcpClient)
            networkStream.Close();
            client.Close();
            
        }
    }
}
