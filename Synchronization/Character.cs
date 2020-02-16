using System.Threading;

namespace Synchronization
{
    public class Character
    {
        private int _armor;
        private int _health = 100;
        public string Name { get; set; }

        public int Health
        {
            get => _health;
            private set => _health = value;
        }

        public int Armor
        {
            get => _armor;
            private set => _armor = value;
        }

        public void Hit(int damage)
        {
            // Health -= damage - Armor;
            int actualDamage = Interlocked.Add(ref damage, -Armor);
            Interlocked.Add(ref _health, -actualDamage);
        }

        public void Heal(int health)
        {
            //Health += health;
            Interlocked.Add(ref _health, health);
        }

        public void CastArmorSpell(bool isPositive)
        {
            if (isPositive)
            {
                Interlocked.Increment(ref _armor);
                //Armor++;
            }
            else
            {
                //Armor--;
                Interlocked.Decrement(ref _armor);
            }
        }
    }


    public class Character2
    {
        private int _armor;
        private int _health = 100;

        public int Health
        {
            get => _health;
            private set => _health = value;
        }

        public int Armor
        {
            get => _armor;
            private set => _armor = value;
        }


        public void Hit(int damage)
        {
             Health -= damage - Armor;
           
        }

        public void Heal(int health)
        {
            Health += health;
           
        }

        public void CastArmorSpell(bool isPositive)
        {
            if (isPositive)
            {
                Interlocked.Increment(ref _armor);
            }
            else
            {
                Interlocked.Decrement(ref _armor);

            }
        }
    }
}