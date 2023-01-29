using ContactManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager.Serialisation
{
    public class BinarySerializer : ISerializer
    {
        public void Serialize(Folder data, string fileName)
        {
            var currentUserMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileLocation = Path.Combine(currentUserMyDocuments, fileName);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Encours de sauvgarder le fichier {fileLocation} ...");
            Console.ResetColor();

            try
            {
                // Demander une cle de chiffrement à l'utilisateur
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">>> Veuiller saisir une cle de chiffrement (16 Characters ou plus): ");
                string key = Console.ReadLine();
                Console.ResetColor();

                // Si aucune cle n'est sécifiée, on utilise le SID de l'utilisateur courant de Windows
                if (string.IsNullOrEmpty(key))
                {
                    var identity = WindowsIdentity.GetCurrent();
                    key = identity.User.Value;
                }

                using (FileStream input = new FileStream(fileLocation, FileMode.Create))
                {
                    //Chiffrement
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] iv = new byte[16];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = keyBytes;
                        aes.IV = iv;

                        using (CryptoStream cryptoStream = new CryptoStream(input, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(cryptoStream, data);
                        }
                    }
                }

                Helper.PrintSuccess($"{fileLocation} est sauvgarde avec succes !");
            }
            catch (FileNotFoundException)
            {
                Helper.PrintError("Le fichier n'est pas trouve ! Veuillez verifier le chemin du fichier !");
            }
            catch (CryptographicException)
            {
                Helper.PrintError("Le chiffrement a echoue, Veuillez verifier la cle et reessayyer");
            }
            catch (IOException)
            {
                Helper.PrintError("Le fichier et encours d'utilisation par un autre processus, veuillez reessayer ulterieurement !");
            }
            catch (SerializationException)
            {
                Helper.PrintError("Il y a un probleme avec le processus de serialisation, Merci de verifier le format du fichier ou bien la cle avant de reessayer !");
            }
        }


        //Dechiffrement
        public Folder Deserialize(string fileName, Folder root, out bool exceptionHandled)
        {
            var currentUserMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileLocation = Path.Combine(currentUserMyDocuments, fileName);

            try
            {
                // demander a l'utilisateur une cle de dechiffrement qui est la meme que la cle de chiffrement AES qu'on a utilise
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">>> Veuillez saisir la cle de dechiffrement: ");
                string key = Console.ReadLine();
                Console.ResetColor();

                // Encore une fois, si aucune cle n'est specifiee, on utilise le SID de l'utilisateur courant du systeme windows
                if (string.IsNullOrEmpty(key))
                {
                    var identity = WindowsIdentity.GetCurrent();
                    key = identity.User.Value;
                }

                using (FileStream input = new FileStream(fileLocation, FileMode.Open))
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] iv = new byte[16];
                    Array.Copy(keyBytes, iv, Math.Min(keyBytes.Length, iv.Length));

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = keyBytes;
                        aes.IV = iv;

                        using (CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            exceptionHandled = false;
                            return (Folder)formatter.Deserialize(cryptoStream);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Helper.PrintError("Le fichier n'est pas trouve ! Veuillez verifier le chemin du fichier !");
                exceptionHandled = true;
                return root;
            }
            catch (CryptographicException)
            {
                Helper.PrintError("Le dechiffrement a echoue, Veuillez verifier la cle et reessayyer");
                exceptionHandled = true;
                return root;
            }
            catch (IOException)
            {
                Helper.PrintError("Le fichier et encours d'utilisation par un autre processus, veuillez reessayer ulterieurement !");
                exceptionHandled = true;
                return root;
            }
            catch (SerializationException)
            {
                Helper.PrintError("Il y a un probleme avec le processus de serialisation, Merci de verifier le format du fichier ou bien la cle avant de reessayer !");
                exceptionHandled = true;
                return root;
            }
        }

    }
}
