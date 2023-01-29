using ContactManager.Data;
using ContactManager.Serialisation;
using System;
using System.IO;

namespace ContactManager
{
    public class Program
    {
        static Folder root = new Folder("Root");
        static Folder currentFolder = root;
        
        
        static void Main(string[] args)
        {
            while (true)
            {
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Choisir une commande parmi le menu suivant : ");
                Console.WriteLine(@"--------------------------------------------");
                Console.WriteLine(@"1. Voir la structure complete              ");
                Console.WriteLine(@"2. Creer un nouveau dossier                 ");
                Console.WriteLine(@"3. Ajouter un contact                       ");
                Console.WriteLine(@"4. Charger                                  ");
                Console.WriteLine(@"5. Sauvgarder                               ");
                Console.WriteLine(@"6. Quitter                                  ");
                Console.WriteLine(@"--------------------------------------------");

                
                //Préparer la saisie de l'utilisateur
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">>> ");

                //Saisir
                var input = Console.ReadLine();
                Console.ResetColor();

                //verifier que l'eutilisateur a saisie quelque chose
                input = (!string.IsNullOrEmpty(input) ? input.ToLower() : string.Empty);

                //les champs separes par espace
                string[] fields = input.Split(' ', StringSplitOptions.None);


                //choisir la commande
                switch (fields[0])
                {
                    //Voire la structure complete
                    case "1":
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Helper.DisplayStructure(root, 0);
                        Console.ResetColor();
                        break;

                    //Creer un nouveau dossier
                    case "2":
                        if (fields.Length > 1)
                        {
                            currentFolder = Helper.CreateFolder(currentFolder, fields[1]);
                        }
                        else
                        {
                            Helper.PrinWarning("Veuillez saisir le nom du dossier");
                        }
                        break;

                    //Ajouter un contact
                    case "3":
                        if (fields.Length > 5)
                        {
                            currentFolder = Helper.AddContact(currentFolder, fields);
                        }
                        //si l'utilisateur n'a pas saisi toutes les informations
                        else
                        {
                            Helper.PrinWarning("Veuillez saisir toutes les informations de contact (Prenom Nom Email Entreprise Lien)");
                        }
                        break;

                    //Charger
                    case "4":
                        var factory = new SerializerFactory();
                        ISerializer serializer;
                        string fileName;
                        Console.WriteLine("Quel type de chargement voulez-vous (Type 1 ou 2): ");
                        Console.WriteLine(" 1.Xml ");
                        Console.WriteLine(" 2.Binaire");

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(">>> ");
                        int? loadingType = int.Parse(Console.ReadLine());
                        Console.ResetColor();

                        //creation du serializer en fonction de l'entree avec le patron de conception Factory
                        if (loadingType == 1)
                        {
                            serializer = factory.CreateSerializer("xml");
                            fileName = "ContactManager1.db";

                        }
                        else if (loadingType == 2)
                        {
                            serializer = factory.CreateSerializer("binaire");
                            fileName = "ContactManager2.db";
                        }
                        else
                        {
                            Helper.PrintError("Instruction inconnue !");
                            break;
                        }

                        // Lors du chargement, l'utilisateur doit saisir le mot de passe qui est fixe par defaut a (contactmanager2023)
                        // Si le mot de passe saisi est incorrect, l'utilisateur a 3 essaies de saisie, a la troisiemme saisie incorrecte la base de donnees ContactManager<1||2>.db est supprimee 
                        string password = "contactmanager2023";
                        
                        //compteur de nombre d'essaies
                        int tryCount = 0;

                        string enteredPassword;
                        while (tryCount < 3)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(">>> Veuillez saisir le mot de passe: ");
                            enteredPassword = Console.ReadLine();
                            Console.ResetColor();

                            if (enteredPassword == password)
                            {
                                // lors du chargement, on verifie si une exception est levee et traitee
                                bool isExceptionHandled;

                                root = serializer.Deserialize(fileName, root, out isExceptionHandled);

                                currentFolder = Helper.FindLastFolder(root);
                                if (!isExceptionHandled)
                                {
                                    Helper.PrintSuccess("La structure est chargee !");
                                }
                                break;
                            }
                            else
                            {
                                tryCount++;

                                Helper.PrintError("Mot de passe incorrect ! Essayer de nouveau !");
                            }
                        }
                        if (tryCount == 3)
                        {
                            // Si le nombre d'essaie = 3 on supprime le fichier de donnees correspondant (1 ou 2)
                            
                            var currentUserMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            
                            var fileLocation = Path.Combine(currentUserMyDocuments, fileName);
                           
                            Console.WriteLine(fileLocation);
                            
                            File.Delete(fileLocation);
                            
                            Helper.PrinWarning("Vous avez atteint le maximum d'essaies (3), le fichier sera supprime !");
                        }

                        break;

                    //Sauvgarder
                    case "5":

                        //un serializer qui sera determine en fonction de l'entree par le patron de conception factory
                        var fact = new SerializerFactory();
                        ISerializer serializer1;
                        
                        Console.WriteLine("Quel type de fichier voulez-vous charger (Type 1 ou 2): ");
                        Console.WriteLine(" 1.Xml ");
                        Console.WriteLine(" 2.Binaire");

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(">>> ");
                        int? type = int.Parse(Console.ReadLine());
                        Console.ResetColor();

                        if (type == 1)
                        {
                            serializer1 = fact.CreateSerializer("xml");
                            serializer1.Serialize(root, "ContactManager1.db");
                        }
                        else if (type == 2)
                        {
                            serializer1 = fact.CreateSerializer("binaire");
                            serializer1.Serialize(root, "ContactManager2.db");
                        }
                        else
                        {
                            Helper.PrintError("Instruction inconnue !");
                            break;
                        }
                        break;

                    //Quitter
                    case "6":
                        return;
                    default:
                        Helper.PrintError("Instruction inconnue !");
                        break;

                }
            }
        }

    }
}
