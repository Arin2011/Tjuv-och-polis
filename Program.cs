using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Schema;

namespace Tjuv_och_polis
{
    //_____________________________________________ MAIN CLASS TjuvOchPolis a small simulations about cops, thiefs and citizens in a town square    
    class TjuvOchPolis
    {
        // ____________________________________________________ DECLARATIONS and STATIC VARIABLES
        enum Sak { Nycklar, Mobil, Pengar, Klocka, Inget };

        public static string[] fstNames = { "Henrik ", "Norman ", "Arin ", "Bert ", "Paddy ", "Pepe ", "Donald ", "Mr ", "Ivanka ", "Lara ", "Jill ", "Kim ", "Sara " };
        public static string[] sndNames = { "Nyström ", "Irmbo ", "Shekelstein ", "Karlsson ", "Hiller ", "Wojack ", "Trump ", "Kay ", "IL SUNG ", "Tiny Boy ", "Ahmedjinidad " };
        public static int amountOfCitizens = 0;
        public static int amountOfThiefs = 0;
        public static int amountOfCops = 0;

        public static Random random = new Random();

        //_____________________________________________________ THE MAP (GRID) DEFAULT SIZE 100 X 25 CELLS
        public const int gridX = 100;
        public const int gridY = 25;
        public static string[,] theGrid = new string[gridX, gridY];

        //_____________________________________________________ THE ANNOUNCMENTBOARD 

        public static string[] announcements = new string[] { "No announcements" };
        public static int anounC = 0;
        public static bool theftDetected = false;
        public static bool thiefCaught = false;

        //_____________________________________________________ STATIC FUNCTIONS
        public static bool spaceOccupied(int x, int y)
        {
            if (theGrid[x, y] != "░")
            {
                return true;
            }//end if occupied
            return false;
        }//end spaceOccupied
        public static void drawGrid()
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    Console.Write(theGrid[x, y]);
                }
                Console.Write("\n");
            }
        }//end drawGrid
        public static void createGrid()
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    theGrid[x, y] = "░";
                }
            }
        }//end createGrid


        public static void drawAnnouncment(string[] events)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════════════════════════╗"); //100 characters

            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].Length < 99)
                {
                    Console.Write("║ ");
                    Console.Write(events[i]);

                    for (int _i = 0; _i < (99 - (events[i].Length)); _i++) { Console.Write(" "); }
                    Console.Write("║\n");
                }//end if within allowed length
                else
                {
                    Console.WriteLine("║ ERROR! EVENT DESCRIPTION TOO LONG  /!'\'║");
                }//end else description of event too long.

            }//end for each string
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════════════════╝");
        }// end drawAnnouncment 

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////// CLASSES                                                                                      CLASSES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //__________________________________________ BASE CLASS PERSON
        class Person
        {
            // _____________________________________________________________________________ INSTANCE VARIABLES
            protected int x;
            protected int y;
            protected int deltaX; // kan vara -1 = går vänstr, 0 = står still, 1 = går höger
            protected int deltaY;
            protected string name;
            protected Sak[] inventory;
            protected bool live;
            // _____________________________________________________________________________ CONSTRUCTOR
            public Person()
            {
                x = random.Next(0, gridX);
                y = random.Next(0, gridY);
                while (spaceOccupied(x, y))
                {
                    x = random.Next(0, gridX);
                    y = random.Next(0, gridY);
                }
                deltaX = random.Next(-1, 1);
                deltaY = random.Next(-1, 1);

                name = "" + (fstNames[(random.Next(0, fstNames.Length))]) + (sndNames[(random.Next(0, sndNames.Length))]);
                inventory = null;
                live = true;
            }//end empty constructor Person
            // _____________________________________________________________________________ GETTERS

            public Sak[] getInv() { return inventory; }
            public string getName() { return name; }
            public int getX() { return x; }
            public int getY() { return y; }

            public bool isAlive() { return live; }
            // _____________________________________________________________________________ FUNCTIONS

            public bool hasStuff()
            {
                for (int i = 0; i < inventory.Length; i++)
                {
                    if (inventory[i] != Sak.Inget) { return true; } //if found a thing! return TRUE
                }//end for
                return false;                                       // if list is empty or only Sak.Inget
            }//end hasStuff
            public void move()
            {                                    //Move, Just moves the person one step
                if (live)
                {
                    x = x + deltaX;
                    y = y + deltaY;
                    // CHECK IF OUTSIDE BORDER 
                    if (x > 99) { x = 0; } // RIGHT    →
                    if (x < 0) { x = 99; } // LEFT     ←
                    if (y > 24) { y = 0; }  // DOWN     ↓
                    if (y < 0) { y = 24; }  // UP       ↑
                }
            }// end void move

            public Sak loose1Item()
            {                               // 1 item out of the inventory gets lost
                if (!hasStuff()) { return Sak.Inget; }
                int x = random.Next(0, inventory.Length);
                while (inventory[x] == Sak.Inget)
                {
                    x = random.Next(0, inventory.Length);           //LOOP UNTIL YOU FIND A NICE ITEM!
                }
                Sak sak = inventory[x];
                inventory[x] = Sak.Inget;
                return sak;                                    // then return it
            }//end loose1Item
        }//end base class person                     END BASE CLASS PERSON

        //__________________________________________ SUB CLASS CITIZEN
        class Citizen : Person
        {
            public Citizen() : base()
            {
                inventory = new Sak[] { Sak.Nycklar, Sak.Mobil, Sak.Pengar, Sak.Klocka };
            }
        }//end sub class Citizen                     _______________END SUB CLASS Citizen

        //__________________________________________ SUB CLASS THIEF
        class Thief : Person
        {
            protected bool criminalScum;
            public Thief() : base()
            {
                inventory = new Sak[(amountOfCitizens * 5)];
                for (int i = 0; i < inventory.Length; i++)
                {
                    inventory[i] = Sak.Inget;
                }
                criminalScum = false;
            }//end empty constructor thief

            public bool isCriminal() { return criminalScum; }
            public void youDoneGoofed()
            {
                criminalScum = true;
                live = false;
            }
            public void release()
            {
                criminalScum = false;
                live = true;
            }

            public void steal(Person p)
            {

                Sak item = p.loose1Item();
                if (item == Sak.Inget)
                {                                        // FAILED THIEVERY!
                    announcements[anounC] = "BUMMER! " + base.getName() + "tried to steal from " +
                                                            p.getName() + " but there was nothing to steal";
                    anounC++;

                }//end if nothing to steal'
                else
                {                                                          // SUCCESSFULL THIEVERY!
                    string s = "" + item;
                    announcements[anounC] = "THEFT! " + base.getName() + "stole a " + s + " from " + p.getName();
                    anounC++;
                    int firstAvalibleSpot = 0;
                    while (inventory[firstAvalibleSpot] != Sak.Inget)
                    {
                        firstAvalibleSpot++;
                    }
                    inventory[firstAvalibleSpot] = item;
                    criminalScum = true;
                    theftDetected = true;
                }// end else
            }//end void steal

        }//end sub class thief         _______________END SUB CLASS THIEF

        //__________________________________________ SUB CLASS COP
        class Cop : Person
        {
            public Cop() : base()
            {
                inventory = new Sak[(amountOfCitizens * 5)];
                for (int i = 0; i < inventory.Length; i++) { inventory[i] = Sak.Inget; }
            }
            public void atemptArrest(Thief t)
            {
                if (t.isCriminal())
                {

                    announcements[anounC] = "Stop right there criminal scum! Nobody breaks the law on my watch! ";
                    anounC++;
                    announcements[anounC] = "I'm confiscating your stolen goods. And then it is off to jail!";
                    anounC++;
                    string s = "";
                    while (t.hasStuff())
                    {
                        Sak stolenGoods = t.loose1Item();
                        int confCounter = 0;
                        while (inventory[confCounter] != Sak.Inget)
                        {
                            confCounter++;
                        }
                        s = s + (" " + stolenGoods);
                        inventory[confCounter] = stolenGoods;
                        if (s.Length > 100)
                        {
                            announcements[anounC] = s;
                            anounC++;
                            s = "";
                        }
                    }//end while loop for the whole list of stolen property!
                    announcements[anounC] = s + "<things confiscated>";
                    anounC++;
                    t.youDoneGoofed();
                    announcements[anounC] = (t.getName() + " was sent to jail ");
                    anounC++;
                    thiefCaught = true;
                }// end BECAUSE IM A CRIMINAL!
            }
        }//end sub class cop             _______________END SUB CLASS COP

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        ////////// MAIN                                                                                                   MAIN
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        static void Main(string[] args)
        {

            createGrid();
            Person[] population;
            Console.WriteLine("How many citizens?");    //then create citizens cops and thievs
            int cit = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many cops?");
            int cops = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("How many thievs?");
            int thievs = Convert.ToInt32(Console.ReadLine());
            Console.Clear();

            population = new Person[(thievs + cops + cit)]; // make spots for them to fill in the population
            int popCounter = 0;
            Thief[] prison = new Thief[(thievs)];
            int[] prisonTime = new int[(thievs)];
            for (int i = 0; i < cit; i++)
            {
                population[popCounter] = new Citizen();
                popCounter++;
                amountOfCitizens++;
            }
            for (int i = 0; i < cops; i++)
            {
                population[popCounter] = new Cop();
                popCounter++;
                amountOfCops++;
            }
            for (int i = 0; i < thievs; i++)
            {
                population[popCounter] = new Thief();
                popCounter++;
                amountOfThiefs++;
            }
            void interaction(Person nr1, Person nr2)
            {
                if ((nr1 is Thief) && (nr2 is Citizen) && (nr1.isAlive()))
                {
                    ((Thief)nr1).steal(nr2);
                }
                else if ((nr1 is Citizen) && (nr2 is Thief) && (nr2.isAlive())) { ((Thief)nr2).steal(nr1); }           //THIEFS STEAL

                else if ((nr1 is Cop) && (nr2 is Thief) && (nr2.isAlive())) { ((Cop)nr1).atemptArrest((Thief)nr2); } //COPS ARREST
                else if ((nr2 is Cop) && (nr1 is Thief) && (nr1.isAlive())) { ((Cop)nr2).atemptArrest((Thief)nr1); }

            }//end void interaction
            void moveOut()
            {
                for (int i = 0; i < population.Length; i++) { population[i].move(); }
            }
            void updateGrid()
            {

                bool conflict = false;
                for (int i = 0; i < population.Length; i++)
                {
                    conflict = false;
                    for (int i2 = i + 1; i2 < population.Length; i2++)
                    {
                        if ((population[i].getX() == population[i2].getX()) && (population[i].getY() == population[i2].getY()))
                        {
                            interaction(population[i], population[i2]);
                            theGrid[(population[i].getX()), (population[i].getY())] = "█";
                            conflict = true;
                        }//end if same square
                    }
                    if (!conflict)
                    {
                        if (population[i] is Thief) { theGrid[population[i].getX(), population[i].getY()] = "T"; }
                        else if (population[i] is Cop) { theGrid[population[i].getX(), population[i].getY()] = "P"; }
                        else if (population[i] is Citizen) { theGrid[(population[i].getX()), (population[i].getY())] = "C"; }
                        else { }
                    }

                }//end for every person in the list
            }//end void updateGrid

            bool prisonCheck(Thief t)
            {
                for (int i = 0; i < prison.Length; i++)
                {
                    if (prison[i] == t) { return true; }
                }
                return false;
            }
            int findPrisonSpot()
            {
                for (int prisonSpot = 0; prisonSpot < prison.Length; prisonSpot++)
                {
                    if (prison[prisonSpot] == null) return prisonSpot;
                }
                return 0;
            }
            void tojmForRelease(int i)
            {
                string prisonerRelease = "" + prison[i].getName();
                prison[i].release();
                prison[i] = null;
                prisonTime[i] = 0;
                announcements[anounC] = "Time in Prison is up for: " + prisonerRelease;
                anounC++;
            }
            void drawPrison()
            {
                //║╚══╔╝╗
                Console.WriteLine("║  P R I S O N                                  ║"); //47 characters
                Console.Write("║");
                for (int prisoners = 0; prisoners < prison.Length; prisoners++)
                {
                    if (prison[prisoners] != null)
                    {
                        string _prisoner = prison[prisoners].getName() + " Serving time left: " + prisonTime[prisoners];
                        for (int asdfasdf = 0; asdfasdf < (46 - _prisoner.Length); asdfasdf++)
                        {
                            _prisoner = _prisoner + " ";
                        }
                        Console.Write(_prisoner + "║\n");
                    }//end if valid prisoner
                }
                Console.WriteLine("╚════════════════════════════════════════════════╝");
            }//void end drawprison


            while (true)
            {
                theftDetected = thiefCaught = false;
                anounC = 0;
                createGrid();
                moveOut();
                updateGrid();
                drawGrid();
                drawAnnouncment(announcements);
                drawPrison();
                if (theftDetected)
                {
                    Console.Beep(5000, 250);
                    Thread.Sleep(200);
                    Console.Beep(5000, 250);
                    Thread.Sleep(2000);
                }
                if (thiefCaught)
                {
                    Console.Beep(5000, 250);
                    Thread.Sleep(200);
                    Console.Beep(6000, 300);
                    Thread.Sleep(4000);
                    for (int i = 0; i < population.Length; i++)
                    {
                        if (!(population[i].isAlive()) && (!prisonCheck((Thief)population[i])))
                        {
                            int x = findPrisonSpot();
                            prison[x] = ((Thief)population[i]);
                            prisonTime[x] = 25;
                        }
                    }//placed everyone in prison!
                }
                for (int ptCountDown = 0; ptCountDown < prison.Length; ptCountDown++)
                {
                    if (prisonTime[ptCountDown] != 0)
                    {
                        prisonTime[ptCountDown] = prisonTime[ptCountDown] - 1;
                        if (prisonTime[ptCountDown] == 1)
                        {
                            tojmForRelease(ptCountDown);
                        }
                    }
                }

                if (announcements[0] == "No new announcements") { Thread.Sleep(500); }
                Console.Clear();
                announcements = new string[10] { "No new announcements", "", "", "", "", "", "", "", "", "" };
            }
        }// END MAIN!
         //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
         //\\\\\\\\\\\\ MAIN                                                                                                   END MAIN
         //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    }//end class TjuvOchPolis

}