
! AVOIR PRIS CONNAISSANCE DU CAHIER DES CHARGES AVANT DE MANIPULER LES DONNEES !

Cette archive contient les données nécessaires au bon déroulement de l'expérimentation sur des
photos à 360°, ainsi que le programme d'expérimentation. Selon l'ordre indiqué par le cahier 
des charges, certaines données spécifiques devront être chargées en amont de l'expérience.

L'archive est constituée :

	- du présent fichier Readme.txt

	- de plusieurs dossiers "DefaultSubjectOrderX". La valeur X est un chiffre représentant
	un ordre de déroulement pour l'expérience, ces ordres sont indiqués dans le cahier des charges.

	- d'un fichier PanoramicExperimentation.txt

	- d'un fichier d'installation .apk

________________________________________________________________________________________________
________________________________________________________________________________________________
________________________________________________________________________________________________


Rappel: POUR EFFECTUER CETTE EXPERIMENTATION, UN OCULUS QUEST EST NECESSAIRE. POUR INSTALLER LE
FICHIER .APK, UTILISER LE LOGICIEL SIDEQUEST. POUR TRANSFERER LES DONNEES, UTILISER L'EXPLORATEUR
DE FICHIERS (WINDOWS), OU UN LOGICIEL DE TRANSFERT DE DONNEES COMPATIBLE ANDROID (MAC). LE
CASQUE DOIT ETRE BRANCHE A L'ORDINATEUR ET ETRE ALLUME.


A. Installer le fichier .apk contenu dans cette archive à l'aide du logiciel SideQuest

B. Mise en place des données sur le Quest :

1.	Avec l'explorateur de fichiers, naviguer jusqu'à la racine du stockage de l'Oculus Quest.

2.	Créer un dossier "360ExperimentationFiles" à la racine (attention, nom sensible à la casse!).
	Si le dossier avait déjà été créé précédemment, ignorer cette étape.

3.	Copier le fichier PanoramicExperimentation.txt dans le dossier précédemment créé. Si le
	fichier avait déjà été copié précédemment, ignorer cette étape.

4.	Copier l'ensemble des dossiers "DefaultSubjectOrderX" dans le dossier précédemment créé. Si les
	dossiers avaient déjà été copié précédemment, ignorer cette étape.

C. Configuration de l'expérience : selon l'ordre de déroulement des blocs souhaité (voir CDC), le
fichier d'expérimentation doit être configuré. Pour cela, ouvrir le fichier "PanoramicExperimentation.txt",
et modifier la valeur de WorkingDirectory, en indiquant le répertoire contenant l'ordre souhaité. Par
défaut, l'ordre rentré est le n°01.
Dans le même fichier, la variable SaveData peut être modifiée en "true" (sauvegarde des résultats activée)
ou en "false" (sauvegarde des résultats désactivée).
! ATTENTION: lors de la modification du fichier, laisser une tabulation entre les propriétés et leur valeur.
Ne pas ajouter de ligne !


D. Récupération des données : les données enregistrées seront sauvegardées dans le dossier
"360ExperimentationFiles/DefaultSubjectOrderX" sur le Quest, sous la forme d'un dossier ayant pour nom, la
date et l'heure d'exécution du protocole.




