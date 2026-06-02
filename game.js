const canvas = document.getElementById("game");
const ctx = canvas.getContext("2d");
const wrap = document.getElementById("gameWrap");
const scoreEl = document.getElementById("score");
const bestEl = document.getElementById("best");
const overlay = document.getElementById("overlay");
const titleEl = document.getElementById("title");
const messageEl = document.getElementById("message");
const startBtn = document.getElementById("start");
const howBtn = document.getElementById("how");
const leftBtn = document.getElementById("left");
const rightBtn = document.getElementById("right");

const W = canvas.width;
const H = canvas.height;
const WORLD_TOP = 5200;
const GRAVITY = 0.46;
const JUMP = -12.2;
const HIGH_JUMP = -17.8;
const WIND_JUMP = -15.2;
const MOVE = 0.82;
const FRICTION = 0.88;
const MAX_X_SPEED = 6.2;

const keys = { left: false, right: false };
let state = "menu";
let cameraY = 0;
let maxClimb = 0;
let best = Number(localStorage.getItem("riverJumpBest") || 0);
let platforms = [];
let particles = [];
let player;
let lastTime = 0;

bestEl.textContent = `Best ${best} m`;

function makePlayer() {
  return { x: W / 2, y: WORLD_TOP - 70, w: 42, h: 46, vx: 0, vy: JUMP, facing: 1, wind: 0 };
}

function resetGame() {
  player = makePlayer();
  cameraY = WORLD_TOP - H;
  maxClimb = 0;
  particles = [];
  platforms = [];
  buildWorld();
  state = "playing";
  overlay.classList.add("hidden");
  lastTime = performance.now();
}

function buildWorld() {
  platforms.push({ type: "log", x: W / 2 - 80, y: WORLD_TOP - 26, w: 160, h: 24 });
  let y = WORLD_TOP - 135;
  let side = 0;

  while (y > 120) {
    const r = Math.random();
    const w = r > 0.82 ? 92 : 118 + Math.random() * 40;
    const x = 26 + Math.random() * (W - w - 52);
    let type = "log";
    if (r > 0.9) type = "wind";
    else if (r > 0.77) type = "mushroom";
    else if (r > 0.62 && y < WORLD_TOP - 650) type = "gator";
    else if (r > 0.54) type = "wave";
    platforms.push({ type, x, y, w, h: type === "gator" ? 28 : 22, broken: false, phase: side });
    y -= 82 + Math.random() * 44;
    side += 0.6;
  }

  platforms.push({ type: "finish", x: 92, y: 46, w: W - 184, h: 26 });
}

function showOverlay(kind) {
  overlay.classList.remove("hidden");
  if (kind === "win") {
    titleEl.textContent = "Kazandin!";
    messageEl.textContent = "Nehri bitirdin. Yeni denemede daha hizli ve temiz cikabilirsin.";
    startBtn.textContent = "Tekrar Oyna";
  } else if (kind === "dead") {
    titleEl.textContent = "Dusunce Bitti";
    messageEl.textContent = "Suya, dalgaya ya da timsahin altina yakalandin. Ustten basmayi dene.";
    startBtn.textContent = "Yeniden Basla";
  } else {
    titleEl.textContent = "Kontroller";
    messageEl.textContent = "Klavye: A/D veya ok tuslari. Telefonda alttaki sol/sag tuslari. Platforma inerken ziplarsin.";
    startBtn.textContent = "Basla";
  }
}

function gameOver() {
  if (state !== "playing") return;
  state = "dead";
  showOverlay("dead");
}

function winGame() {
  if (state !== "playing") return;
  state = "win";
  showOverlay("win");
}

function addBurst(x, y, color, count = 14) {
  for (let i = 0; i < count; i++) {
    particles.push({ x, y, vx: (Math.random() - 0.5) * 6, vy: -Math.random() * 4 - 1, life: 30 + Math.random() * 20, color });
  }
}

function update() {
  if (state !== "playing") return;

  const accel = (keys.right ? MOVE : 0) - (keys.left ? MOVE : 0);
  player.vx += accel;
  player.vx *= FRICTION;
  player.vx = Math.max(-MAX_X_SPEED, Math.min(MAX_X_SPEED, player.vx));
  if (Math.abs(player.vx) > 0.15) player.facing = Math.sign(player.vx);

  player.vy += GRAVITY;
  player.x += player.vx;
  player.y += player.vy;

  if (player.x < -player.w) player.x = W;
  if (player.x > W) player.x = -player.w;

  if (player.wind > 0) {
    player.wind -= 1;
    player.vy -= 0.18;
    addBurst(player.x, player.y + player.h, "rgba(255,255,255,0.7)", 1);
  }

  collidePlatforms();

  const target = player.y - H * 0.42;
  cameraY += (target - cameraY) * 0.08;
  cameraY = Math.max(0, Math.min(cameraY, WORLD_TOP - H));

  const climbed = Math.max(0, Math.floor((WORLD_TOP - player.y) / 10));
  maxClimb = Math.max(maxClimb, climbed);
  scoreEl.textContent = `${maxClimb} m`;
  if (maxClimb > best) {
    best = maxClimb;
    localStorage.setItem("riverJumpBest", best);
    bestEl.textContent = `Best ${best} m`;
  }

  particles = particles.filter((p) => {
    p.x += p.vx;
    p.y += p.vy;
    p.vy += 0.12;
    p.life -= 1;
    return p.life > 0;
  });

  if (player.y - cameraY > H + 90) gameOver();
  if (player.y < 88) winGame();
}

function collidePlatforms() {
  const feet = player.y + player.h / 2;
  const prevFeet = feet - player.vy;
  for (const p of platforms) {
    if (p.broken) continue;
    const hitX = player.x + player.w / 2 > p.x && player.x - player.w / 2 < p.x + p.w;
    const crossingTop = prevFeet <= p.y && feet >= p.y;
    const closeY = feet >= p.y && feet <= p.y + p.h + 18;
    if (!hitX || !closeY) continue;

    if (p.type === "wave") return gameOver();
    if (p.type === "gator" && !crossingTop) return gameOver();

    if (player.vy > 0 && crossingTop) {
      player.y = p.y - player.h / 2;
      if (p.type === "mushroom") {
        player.vy = HIGH_JUMP;
        addBurst(player.x, p.y, "#f65f5f", 18);
      } else if (p.type === "wind") {
        player.vy = WIND_JUMP;
        player.wind = 180;
        addBurst(player.x, p.y, "#ffffff", 24);
      } else if (p.type === "gator") {
        player.vy = JUMP * 0.95;
        p.broken = true;
        addBurst(p.x + p.w / 2, p.y, "#678d58", 28);
      } else if (p.type === "finish") {
        winGame();
      } else {
        player.vy = JUMP;
        addBurst(player.x, p.y, "#8fd36a", 8);
      }
      return;
    }
  }
}

function draw() {
  ctx.clearRect(0, 0, W, H);
  drawBackground();
  ctx.save();
  ctx.translate(0, -cameraY);
  platforms.forEach(drawPlatform);
  drawPlayer(player);
  drawParticles();
  ctx.restore();
  drawGoalRibbon();
}

function drawBackground() {
  const waterOffset = (performance.now() * 0.035) % 80;
  ctx.fillStyle = "#bceeff";
  ctx.fillRect(0, 0, W, H);
  ctx.fillStyle = "rgba(255,255,255,0.5)";
  for (let i = -1; i < 9; i++) waveLine(0, i * 96 + waterOffset, W, 14);
  ctx.fillStyle = "rgba(64, 154, 185, 0.16)";
  for (let i = 0; i < 11; i++) waveLine(0, i * 78 - waterOffset, W, 8);
}

function waveLine(x, y, width, amp) {
  ctx.beginPath();
  ctx.moveTo(x, y);
  for (let px = x; px <= x + width; px += 24) ctx.quadraticCurveTo(px + 12, y - amp, px + 24, y);
  ctx.lineTo(x + width, y + 12);
  ctx.lineTo(x, y + 12);
  ctx.closePath();
  ctx.fill();
}

function drawGoalRibbon() {
  const y = 16 - cameraY;
  if (y < -70 || y > 120) return;
  ctx.fillStyle = "#fff";
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 4;
  ctx.beginPath();
  ctx.moveTo(32, y + 22);
  ctx.quadraticCurveTo(W / 2, y - 10, W - 32, y + 22);
  ctx.lineTo(W - 32, y + 48);
  ctx.quadraticCurveTo(W / 2, y + 16, 32, y + 48);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.fillStyle = "#243047";
  ctx.font = "900 24px Trebuchet MS";
  ctx.textAlign = "center";
  ctx.fillText("REACH THE END TO WIN", W / 2, y + 38);
}

function drawPlatform(p) {
  if (p.broken) return;
  if (p.type === "log") drawLog(p);
  if (p.type === "mushroom") drawMushroom(p);
  if (p.type === "wind") drawWind(p);
  if (p.type === "gator") drawGator(p);
  if (p.type === "wave") drawHazardWave(p);
  if (p.type === "finish") drawFinish(p);
}

function drawLog(p) {
  ctx.fillStyle = "#8d633f";
  roundRect(p.x, p.y, p.w, p.h, 8);
  ctx.fill();
  ctx.strokeStyle = "#4b3322";
  ctx.lineWidth = 3;
  ctx.stroke();
  ctx.fillStyle = "#6f4b30";
  ctx.fillRect(p.x + 14, p.y + 6, p.w - 28, 3);
}

function drawMushroom(p) {
  drawLog(p);
  const cx = p.x + p.w / 2;
  ctx.fillStyle = "#f8efe1";
  roundRect(cx - 18, p.y - 36, 36, 38, 8);
  ctx.fill();
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 3;
  ctx.stroke();
  ctx.fillStyle = "#d94e52";
  ctx.beginPath();
  ctx.moveTo(cx - 48, p.y - 28);
  ctx.quadraticCurveTo(cx, p.y - 76, cx + 48, p.y - 28);
  ctx.quadraticCurveTo(cx + 42, p.y - 10, cx - 42, p.y - 10);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.fillStyle = "#fff7df";
  for (const dx of [-24, 0, 24]) {
    ctx.beginPath();
    ctx.arc(cx + dx, p.y - 31 - Math.abs(dx) * 0.25, 7, 0, Math.PI * 2);
    ctx.fill();
  }
}

function drawWind(p) {
  ctx.fillStyle = "rgba(255,255,255,0.82)";
  roundRect(p.x, p.y - 6, p.w, p.h + 12, 8);
  ctx.fill();
  ctx.strokeStyle = "#4d9ec0";
  ctx.lineWidth = 3;
  ctx.stroke();
  ctx.fillStyle = "#2c7da0";
  ctx.font = "900 25px Trebuchet MS";
  ctx.textAlign = "center";
  for (let i = 0; i < 5; i++) ctx.fillText("↑", p.x + 18 + i * ((p.w - 36) / 4), p.y + 20);
}

function drawGator(p) {
  ctx.fillStyle = "#6fa85d";
  ctx.beginPath();
  ctx.moveTo(p.x + 8, p.y + p.h);
  ctx.lineTo(p.x + p.w - 18, p.y + p.h);
  ctx.lineTo(p.x + p.w + 6, p.y + 10);
  ctx.lineTo(p.x + p.w - 38, p.y + 4);
  ctx.quadraticCurveTo(p.x + 38, p.y - 8, p.x + 8, p.y + p.h);
  ctx.closePath();
  ctx.fill();
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 3;
  ctx.stroke();
  ctx.fillStyle = "#fff";
  ctx.beginPath();
  ctx.arc(p.x + p.w - 32, p.y + 5, 8, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();
  ctx.fillStyle = "#243047";
  ctx.beginPath();
  ctx.arc(p.x + p.w - 30, p.y + 5, 3, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "#fff";
  for (let i = 0; i < 5; i++) {
    ctx.beginPath();
    ctx.moveTo(p.x + p.w - 60 + i * 10, p.y + p.h);
    ctx.lineTo(p.x + p.w - 55 + i * 10, p.y + p.h - 10);
    ctx.lineTo(p.x + p.w - 50 + i * 10, p.y + p.h);
    ctx.fill();
  }
}

function drawHazardWave(p) {
  ctx.fillStyle = "#2c7da0";
  ctx.beginPath();
  ctx.moveTo(p.x, p.y + p.h);
  for (let x = p.x; x <= p.x + p.w; x += 28) ctx.quadraticCurveTo(x + 14, p.y - 28, x + 28, p.y + p.h);
  ctx.lineTo(p.x, p.y + p.h);
  ctx.fill();
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 3;
  ctx.stroke();
}

function drawFinish(p) {
  ctx.fillStyle = "#fff7df";
  roundRect(p.x, p.y, p.w, p.h, 8);
  ctx.fill();
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 4;
  ctx.stroke();
  ctx.fillStyle = "#243047";
  ctx.font = "900 18px Trebuchet MS";
  ctx.textAlign = "center";
  ctx.fillText("END", p.x + p.w / 2, p.y + 20);
}

function drawPlayer(frog) {
  if (!frog) return;
  ctx.save();
  ctx.translate(frog.x, frog.y);
  ctx.scale(frog.facing, 1);
  ctx.fillStyle = "#76c86b";
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.ellipse(0, 6, 21, 25, 0, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(-12, -15, 10, 0, Math.PI * 2);
  ctx.arc(12, -15, 10, 0, Math.PI * 2);
  ctx.fill();
  ctx.stroke();
  ctx.fillStyle = "#fff";
  ctx.beginPath();
  ctx.arc(-12, -17, 5, 0, Math.PI * 2);
  ctx.arc(12, -17, 5, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "#243047";
  ctx.beginPath();
  ctx.arc(-10, -17, 2.4, 0, Math.PI * 2);
  ctx.arc(14, -17, 2.4, 0, Math.PI * 2);
  ctx.fill();
  ctx.strokeStyle = "#243047";
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.arc(0, 3, 11, 0.15, Math.PI - 0.15);
  ctx.stroke();
  ctx.beginPath();
  ctx.ellipse(-21, 25, 11, 6, -0.45, 0, Math.PI * 2);
  ctx.ellipse(21, 25, 11, 6, 0.45, 0, Math.PI * 2);
  ctx.fillStyle = "#5eb75f";
  ctx.fill();
  ctx.stroke();
  ctx.restore();
}

function drawParticles() {
  for (const p of particles) {
    ctx.globalAlpha = Math.max(0, Math.min(1, p.life / 35));
    ctx.fillStyle = p.color;
    ctx.beginPath();
    ctx.arc(p.x, p.y, 3, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1;
}

function roundRect(x, y, w, h, r) {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.lineTo(x + w - r, y);
  ctx.quadraticCurveTo(x + w, y, x + w, y + r);
  ctx.lineTo(x + w, y + h - r);
  ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
  ctx.lineTo(x + r, y + h);
  ctx.quadraticCurveTo(x, y + h, x, y + h - r);
  ctx.lineTo(x, y + r);
  ctx.quadraticCurveTo(x, y, x + r, y);
  ctx.closePath();
}

function loop(now) {
  if (now - lastTime > 12) {
    update();
    draw();
    lastTime = now;
  }
  requestAnimationFrame(loop);
}

function setKey(name, down) { keys[name] = down; }

window.addEventListener("keydown", (event) => {
  if (["ArrowLeft", "a", "A"].includes(event.key)) setKey("left", true);
  if (["ArrowRight", "d", "D"].includes(event.key)) setKey("right", true);
  if (event.key === " " && state !== "playing") resetGame();
});

window.addEventListener("keyup", (event) => {
  if (["ArrowLeft", "a", "A"].includes(event.key)) setKey("left", false);
  if (["ArrowRight", "d", "D"].includes(event.key)) setKey("right", false);
});

function bindPad(button, name) {
  button.addEventListener("pointerdown", (event) => {
    event.preventDefault();
    button.setPointerCapture(event.pointerId);
    setKey(name, true);
  });
  button.addEventListener("pointerup", () => setKey(name, false));
  button.addEventListener("pointercancel", () => setKey(name, false));
  button.addEventListener("pointerleave", () => setKey(name, false));
}

bindPad(leftBtn, "left");
bindPad(rightBtn, "right");
startBtn.addEventListener("click", resetGame);
howBtn.addEventListener("click", () => showOverlay("help"));
wrap.addEventListener("pointerdown", () => { if (state !== "playing") resetGame(); });

player = makePlayer();
buildWorld();
draw();
requestAnimationFrame(loop);
