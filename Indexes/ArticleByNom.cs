using YesSql.Indexes;
using YesSql.Samples.FullText.Models;

public class ArticleByNom : MapIndex// ReduceIndex ou MapIndex
{
    public string Nom { get; set; }
    //public int NomIsNull => Nom == null ? 1 : 0;
    // Cette propriété doit être stockée en base,
    // donc on lui ajoute un setter.
    public int NomIsNull { get; set; }

}
/*Déclaration d’une propriété en lecture seule

public int NomIsNull => ...;

Le symbole => définit une expression-bodied property (une syntaxe courte de C# pour les getters).

Cela équivaut à écrire :
public int NomIsNull
{
    get { return Nom == null ? 1 : 0; }
}
Donc NomIsNull est une propriété calculée automatiquement à la lecture, pas stockée directement dans la base.*/
/*
int valeur;
if (Nom == null)
    valeur = 1;
else
    valeur = 0;*/


