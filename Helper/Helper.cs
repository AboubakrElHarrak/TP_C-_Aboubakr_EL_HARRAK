using ContactManager.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ContactManager
{
    public static class Helper
    {
        // pour le traitement de la case
        static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        public static Folder AddContact(Folder currentFolder, string[] fields)
        {
            // verifier le format de l'adresse mail
            try
            {
                var addr = new System.Net.Mail.MailAddress(fields[3]);
            }
            catch (FormatException)
            {
                PrintError("Format invalid de l'adresse e-mail");

                return currentFolder;
            }

            string[] links = new string[4] { "ami", "collegue", "relation", "reseau" };
            ContactLink _link;

            // Verifier que le lien correspond bien a un element de l'enum de liens
            if (links.Contains(fields[5]))
            {
                // Convertir un string en type de l'enum de liens de contacts
                _link = (ContactLink)Enum.Parse(typeof(ContactLink), textInfo.ToTitleCase(fields[5]));

                // Creation d'un nouveau contact avec le constructeur de la classe Contact en faisant passer les 5 params
                var newContact = new Contact(textInfo.ToTitleCase(fields[1]), textInfo.ToTitleCase(fields[2]), fields[3],
                   textInfo.ToTitleCase(fields[4]), _link);

                // Ajouter le nouveau contact au dossier
                currentFolder.Contacts.Add(newContact);

                PrintSuccess("Contact ajouté avec succes !");
            }
            else
            {
                PrintError("Format de lien invalid (Choisir entre : Ami || Collegue || Relation || Reseau)");
            }

            return currentFolder;
        }

        public static Folder CreateFolder(Folder currentFolder, string nameFolder)
        {
            var newFolder = new Folder(textInfo.ToTitleCase(nameFolder));

            currentFolder.ChildFolders.Add(newFolder);
            // Current folder point on the last folder created
            currentFolder = newFolder;

            PrintSuccess("Dossier cree avec succes !");


            return currentFolder;

        }


        //Pour l'affichage de la structure de l'objet
        public static void DisplayStructure(Folder folder, int depth)
        {
            // On utilisera la ricurcivité pour afficher la structure
            Console.WriteLine(new string('-', depth) + folder.ToString());

            foreach (var subFolder in folder.ChildFolders)
            {
                DisplayStructure(subFolder, depth + 1);
            }
            foreach (var contact in folder.Contacts)
            {
                Console.WriteLine(new string('-', depth + 1) + contact.ToString());
            }
        }

        // Cette methode sera utilisee pour retrouver le dernier dossier lorsqu'on a un chargement 
        public static Folder FindLastFolder(Folder folder)
        {
            Folder lastFolder = folder;
            if (folder.ChildFolders.Count > 0)
            {
                lastFolder = folder.ChildFolders.OrderByDescending(f => f.CreationDate).First();
                lastFolder = FindLastFolder(lastFolder);
            }
            return lastFolder;
        }

        // Cette Methode centralise les memes instructions communes entre toutes les erreurs et prend en param un message qui sera affiche dans la console
        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Erreur: " + message);
        }

        // Cette Methode centralise les memes instructions communes entre toutes les avertissements et prend en param un message qui sera affiche dans la console
        public static void PrinWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Avertissement: " + message);
        }

        // Cette Methode centralise les memes instructions communes entre toutes les succes et prend en param un message qui sera affiche dans la console
        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Success: " + message);
        }
    }
}
