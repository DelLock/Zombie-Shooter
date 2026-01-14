using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shoot_Out_Game_MOO_ICT
{
    public partial class Form1 : Form
    {
        bool goLeft, goRight, goUp, goDown, gameOver;
        string facing = "up";
        int playerHealth = 100;
        int speed = 10;
        int ammo = 10;
        double zombieSpeed = 3.0;
        Random randNum = new Random();
        int score;
        List<PictureBox> zombiesList = new List<PictureBox>();

        // Волновая система
        private int wave = 1;
        private int waveTime = 60000;
        private int waveTimer = 0;
        private bool isResting = false;
        private int restTime = 15000;
        private int restTimer = 0;
        private int zombieCount = 3;
        private int maxZombies = 15;
        private bool normalAmmoSpawn = true;

        // UI элементы
        private Label txtWave;
        private Label txtTime;
        private Label txtRest;
        private Label txtMessage;
        private List<PictureBox> ammoBoxes = new List<PictureBox>();
        private List<PictureBox> healthBoxes = new List<PictureBox>();

        public Form1()
        {
            InitializeComponent();
            InitializeWaveLabels();
            InitializeMessageLabel();
            RestartGame();
        }

        private void InitializeWaveLabels()
        {
            // Метка волны
            txtWave = new Label();
            txtWave.AutoSize = true;
            txtWave.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            txtWave.ForeColor = Color.Yellow;
            txtWave.Location = new Point(200, 13);
            txtWave.Text = "Wave: 1";
            this.Controls.Add(txtWave);
            txtWave.BringToFront();

            // Метка времени волны
            txtTime = new Label();
            txtTime.AutoSize = true;
            txtTime.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            txtTime.ForeColor = Color.LightGreen;
            txtTime.Location = new Point(350, 13);
            txtTime.Text = "Time: 60";
            this.Controls.Add(txtTime);
            txtTime.BringToFront();

            // Метка отдыха
            txtRest = new Label();
            txtRest.AutoSize = true;
            txtRest.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            txtRest.ForeColor = Color.Cyan;
            txtRest.Location = new Point(500, 13);
            txtRest.Text = "Rest: 15";
            txtRest.Visible = false;
            this.Controls.Add(txtRest);
            txtRest.BringToFront();
        }

        private void InitializeMessageLabel()
        {
            txtMessage = new Label();
            txtMessage.AutoSize = false;
            txtMessage.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            txtMessage.ForeColor = Color.White;
            txtMessage.BackColor = Color.FromArgb(150, 0, 0, 0);
            txtMessage.Size = new Size(600, 100);
            txtMessage.Location = new Point(500, 300);
            txtMessage.TextAlign = ContentAlignment.MiddleCenter;
            txtMessage.Visible = false;
            this.Controls.Add(txtMessage);
            txtMessage.BringToFront();
        }

        private void ShowMessage(string message, int duration = 2000)
        {
            txtMessage.Text = message;
            txtMessage.Visible = true;
            txtMessage.BringToFront();

            Timer messageTimer = new Timer();
            messageTimer.Interval = duration;
            messageTimer.Tick += (s, e) => {
                txtMessage.Visible = false;
                messageTimer.Stop();
                messageTimer.Dispose();
            };
            messageTimer.Start();
        }

        private void MainTimerEvent(object sender, EventArgs e)
        {
            // Обновление таймеров волн
            UpdateWaveTimers();

            if (playerHealth > 1)
            {
                healthBar.Value = playerHealth;
            }
            else
            {
                gameOver = true;
                player.Image = Properties.Resources.dead;
                GameTimer.Stop();
                ShowMessage($"Game Over!\nSurvived {wave - 1} waves\nKills: {score}", 5000);
                return;
            }

            txtAmmo.Text = "Ammo: " + ammo;
            txtScore.Text = "Kills: " + score;
            txtWave.Text = "Wave: " + wave;

            // Отображаем время в секундах
            int timeLeftInSeconds = (waveTime - waveTimer) / 50;
            txtTime.Text = "Time: " + Math.Max(0, timeLeftInSeconds);

            // Движение игрока
            if (goLeft == true && player.Left > 0)
            {
                player.Left -= speed;
            }
            if (goRight == true && player.Left + player.Width < this.ClientSize.Width)
            {
                player.Left += speed;
            }
            if (goUp == true && player.Top > 45)
            {
                player.Top -= speed;
            }
            if (goDown == true && player.Top + player.Height < this.ClientSize.Height)
            {
                player.Top += speed;
            }

            // Обработка столкновений
            ProcessCollisions();

            // Проверка зомби
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "zombie")
                {
                    MoveZombie((PictureBox)x);
                }
            }

            // Проверка попадания пуль
            CheckBulletHits();

            // Игрок всегда сверху
            player.BringToFront();
        }

        private void UpdateWaveTimers()
        {
            if (!isResting && !gameOver)
            {
                waveTimer += 20;

                if (waveTimer >= waveTime)
                {
                    CompleteWave();
                }
            }
            else if (isResting && !gameOver)
            {
                restTimer += 20;

                // Отображаем время отдыха в секундах
                int restTimeLeftInSeconds = (restTime - restTimer) / 50;
                txtRest.Text = "Rest: " + Math.Max(0, restTimeLeftInSeconds);

                if (restTimer >= restTime)
                {
                    StartNewWave();
                }
            }
        }

        private void SkipCurrentWave()
        {
            if (!gameOver && !isResting)
            {
                // МГНОВЕННО завершаем волну
                waveTimer = waveTime;
                CompleteWave();
            }
        }

        private void SkipCurrentRest()
        {
            if (!gameOver && isResting)
            {
                // МГНОВЕННО пропускаем отдых
                restTimer = restTime;
                StartNewWave();
            }
        }

        private void CompleteWave()
        {
            // Волна завершена
            isResting = true;
            restTimer = 0;
            normalAmmoSpawn = false;
            txtRest.Visible = true;

            // Показываем сообщение о завершении волны
            ShowMessage($"Wave {wave} Complete!");

            // Полностью очищаем карту
            ClearAllGameObjects();

            // Спавним предметы для отдыха
            SpawnRestItems();
        }

        private void StartNewWave()
        {
            // Полностью очищаем карту перед новой волной
            ClearAllGameObjects();

            // Начинаем новую волну
            isResting = false;
            waveTimer = 0;
            wave++;

            // Увеличиваем время волны на 10 секунд
            waveTime += 10000;

            // Увеличиваем скорость зомби на 0.3
            zombieSpeed += 0.3;

            // Увеличиваем количество зомби
            zombieCount = Math.Min(3 + wave, maxZombies);

            normalAmmoSpawn = true;
            txtRest.Visible = false;

            // Показываем сообщение о начале новой волны
            ShowMessage($"Wave {wave} Started!");

            // Спавним зомби для новой волны
            for (int i = 0; i < zombieCount; i++)
            {
                MakeZombies();
            }
        }

        private void SpawnRestItems()
        {
            // Спавним аптечку (полное восстановление)
            PictureBox healthPack = new PictureBox();
            healthPack.Image = Properties.Resources.up;
            healthPack.BackColor = Color.Lime;
            healthPack.Size = new Size(40, 40);
            healthPack.SizeMode = PictureBoxSizeMode.StretchImage;
            healthPack.Left = randNum.Next(100, this.ClientSize.Width - 140);
            healthPack.Top = randNum.Next(150, this.ClientSize.Height - 140);
            healthPack.Tag = "health";
            this.Controls.Add(healthPack);
            healthPack.BringToFront();
            healthBoxes.Add(healthPack);

            // Спавним 10 патронов
            for (int i = 0; i < 10; i++)
            {
                PictureBox ammoBox = new PictureBox();
                ammoBox.Image = Properties.Resources.ammo_Image;
                ammoBox.SizeMode = PictureBoxSizeMode.AutoSize;

                int x = randNum.Next(50, this.ClientSize.Width - 100);
                int y = randNum.Next(100, this.ClientSize.Height - 100);

                bool validPosition = true;
                foreach (PictureBox existingAmmo in ammoBoxes)
                {
                    if (Math.Abs(x - existingAmmo.Left) < 50 && Math.Abs(y - existingAmmo.Top) < 50)
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    ammoBox.Left = x;
                    ammoBox.Top = y;
                    ammoBox.Tag = "ammo";
                    this.Controls.Add(ammoBox);
                    ammoBox.BringToFront();
                    ammoBoxes.Add(ammoBox);
                }
            }
        }

        private void ProcessCollisions()
        {
            List<Control> toRemove = new List<Control>();

            foreach (Control x in this.Controls)
            {
                // Сбор патронов
                if (x is PictureBox && (string)x.Tag == "ammo")
                {
                    if (player.Bounds.IntersectsWith(x.Bounds))
                    {
                        toRemove.Add(x);
                        ammo += 5;
                    }
                }

                // Сбор аптечек (только во время отдыха)
                if (x is PictureBox && (string)x.Tag == "health" && isResting)
                {
                    if (player.Bounds.IntersectsWith(x.Bounds))
                    {
                        toRemove.Add(x);
                        playerHealth = 100;
                        healthBar.Value = playerHealth;
                        ShowMessage("Health Restored!");
                    }
                }
            }

            // Удаляем собранные предметы
            foreach (Control item in toRemove)
            {
                this.Controls.Remove(item);
                item.Dispose();

                if (item is PictureBox)
                {
                    if ((string)item.Tag == "ammo")
                        ammoBoxes.Remove((PictureBox)item);
                    else if ((string)item.Tag == "health")
                        healthBoxes.Remove((PictureBox)item);
                }
            }
        }

        private void MoveZombie(PictureBox zombie)
        {
            if (player.Bounds.IntersectsWith(zombie.Bounds) && !isResting)
            {
                playerHealth -= 1;
            }

            if (!isResting)
            {
                int moveSpeed = (int)Math.Round(zombieSpeed);

                if (zombie.Left > player.Left)
                {
                    zombie.Left -= moveSpeed;
                    zombie.Image = Properties.Resources.zleft;
                }
                if (zombie.Left < player.Left)
                {
                    zombie.Left += moveSpeed;
                    zombie.Image = Properties.Resources.zright;
                }
                if (zombie.Top > player.Top)
                {
                    zombie.Top -= moveSpeed;
                    zombie.Image = Properties.Resources.zup;
                }
                if (zombie.Top < player.Top)
                {
                    zombie.Top += moveSpeed;
                    zombie.Image = Properties.Resources.zdown;
                }
            }
        }

        private void CheckBulletHits()
        {
            List<Control> bulletsToRemove = new List<Control>();
            List<Control> zombiesToRemove = new List<Control>();

            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "zombie")
                {
                    foreach (Control j in this.Controls)
                    {
                        if (j is PictureBox && (string)j.Tag == "bullet")
                        {
                            if (x.Bounds.IntersectsWith(j.Bounds))
                            {
                                score++;
                                bulletsToRemove.Add(j);
                                zombiesToRemove.Add(x);
                            }
                        }
                    }
                }
            }

            foreach (Control bullet in bulletsToRemove)
            {
                this.Controls.Remove(bullet);
                bullet.Dispose();
            }

            foreach (Control zombie in zombiesToRemove)
            {
                this.Controls.Remove(zombie);
                zombie.Dispose();
                zombiesList.Remove((PictureBox)zombie);

                if (!isResting && zombiesList.Count < zombieCount)
                {
                    MakeZombies();
                }
            }
        }

        private void ClearAllGameObjects()
        {
            // Очищаем зомби
            foreach (PictureBox zombie in zombiesList.ToList())
            {
                this.Controls.Remove(zombie);
                zombie.Dispose();
            }
            zombiesList.Clear();

            // Очищаем патроны
            foreach (PictureBox ammoBox in ammoBoxes.ToList())
            {
                this.Controls.Remove(ammoBox);
                ammoBox.Dispose();
            }
            ammoBoxes.Clear();

            // Очищаем аптечки
            foreach (PictureBox healthBox in healthBoxes.ToList())
            {
                this.Controls.Remove(healthBox);
                healthBox.Dispose();
            }
            healthBoxes.Clear();

            // Очищаем все пули
            List<Control> bulletsToRemove = new List<Control>();
            foreach (Control c in this.Controls)
            {
                if (c is PictureBox && (string)c.Tag == "bullet")
                {
                    bulletsToRemove.Add(c);
                }
            }
            foreach (Control bullet in bulletsToRemove)
            {
                this.Controls.Remove(bullet);
                bullet.Dispose();
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (gameOver == true)
            {
                return;
            }

            // Управление стрелками ИЛИ WASD
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                goLeft = true;
                facing = "left";
                player.Image = Properties.Resources.left;
            }

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                goRight = true;
                facing = "right";
                player.Image = Properties.Resources.right;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                goUp = true;
                facing = "up";
                player.Image = Properties.Resources.up;
            }

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                goDown = true;
                facing = "down";
                player.Image = Properties.Resources.down;
            }

            // Пропуск волны/отдыха по клавише + (Numpad + или обычный +)
            if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
            {
                if (!isResting && !gameOver)
                {
                    // МГНОВЕННО пропускаем волну
                    SkipCurrentWave();
                }
                else if (isResting && !gameOver)
                {
                    // МГНОВЕННО пропускаем отдых
                    SkipCurrentRest();
                }
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            // Управление стрелками ИЛИ WASD
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                goLeft = false;
            }

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                goRight = false;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                goUp = false;
            }

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                goDown = false;
            }

            // Стрельба пробелом
            if (e.KeyCode == Keys.Space && ammo > 0 && gameOver == false)
            {
                ammo--;
                ShootBullet(facing);

                if (ammo < 5 && normalAmmoSpawn && randNum.Next(1, 10) > 7 && !isResting)
                {
                    DropAmmo();
                }
            }

            // Перезапуск игры
            if (e.KeyCode == Keys.Enter && gameOver == true)
            {
                RestartGame();
            }
        }

        private void ShootBullet(string direction)
        {
            Bullet shootBullet = new Bullet();
            shootBullet.direction = direction;
            shootBullet.bulletLeft = player.Left + (player.Width / 2);
            shootBullet.bulletTop = player.Top + (player.Height / 2);
            shootBullet.MakeBullet(this);
        }

        private void MakeZombies()
        {
            PictureBox zombie = new PictureBox();
            zombie.Tag = "zombie";
            zombie.Image = Properties.Resources.zdown;

            int spawnX, spawnY;
            int attempts = 0;
            do
            {
                spawnX = randNum.Next(100, this.ClientSize.Width - 150);
                spawnY = randNum.Next(150, this.ClientSize.Height - 150);
                attempts++;

                if (attempts > 30) break;

            } while (Math.Abs(spawnX - player.Left) < 300 && Math.Abs(spawnY - player.Top) < 300);

            zombie.Left = spawnX;
            zombie.Top = spawnY;
            zombie.SizeMode = PictureBoxSizeMode.AutoSize;
            zombiesList.Add(zombie);
            this.Controls.Add(zombie);
            player.BringToFront();
        }

        private void DropAmmo()
        {
            if (!normalAmmoSpawn || isResting) return;

            PictureBox ammoBox = new PictureBox();
            ammoBox.Image = Properties.Resources.ammo_Image;
            ammoBox.SizeMode = PictureBoxSizeMode.AutoSize;

            ammoBox.Left = randNum.Next(100, this.ClientSize.Width - 150);
            ammoBox.Top = randNum.Next(150, this.ClientSize.Height - 150);
            ammoBox.Tag = "ammo";

            this.Controls.Add(ammoBox);
            ammoBoxes.Add(ammoBox);

            ammoBox.BringToFront();
            player.BringToFront();
        }

        private void RestartGame()
        {
            player.Image = Properties.Resources.up;

            // Полностью очищаем все игровые объекты
            ClearAllGameObjects();

            // Сброс волновой системы
            wave = 1;
            waveTime = 60000;
            waveTimer = 0;
            isResting = false;
            restTimer = 0;
            zombieSpeed = 3.0;
            zombieCount = 3;
            normalAmmoSpawn = true;

            if (txtRest != null)
            {
                txtRest.Visible = false;
                txtRest.Text = "Rest: 15";
            }
            if (txtTime != null) txtTime.Text = "Time: 60";
            if (txtWave != null) txtWave.Text = "Wave: 1";
            if (txtMessage != null) txtMessage.Visible = false;

            // Спавним начальных зомби
            for (int i = 0; i < zombieCount; i++)
            {
                MakeZombies();
            }

            goUp = false;
            goDown = false;
            goLeft = false;
            goRight = false;
            gameOver = false;

            playerHealth = 100;
            healthBar.Value = 100;
            score = 0;
            ammo = 10;

            // Показываем приветственное сообщение
            ShowMessage("Get Ready!\nWave 1 Starting...", 2000);

            GameTimer.Start();
        }
    }
}