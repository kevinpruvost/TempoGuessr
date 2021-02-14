/*
#
# HEADER
#
*/

var canvas;

var draggables = new Array(3);

var dragged = null;
var offsetX;
var offsetY;
var draggedOrigin;

var backgroundImage;

var JSONviews = null;

var currentView = -1;

var btnStart;
var HUDContainer;
var HUDbtnNext;
var HUDnbImage;
var HUDScore;

var nbImage = 1;
var score = 0;

const STATES = Object.freeze({"WAITING":'waiting', "DRAGGING":'dragging', "CHECKED":'checked'});
var playerState = STATES.WAITING;

function loadImages()
{
  currentView = floor(random(JSONviews.views.length));
  var indexs = [0, 1, 2];

  shuffle(indexs, true);

  for (let i = 0; i < 3; ++i) {
    //draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage('../TempoGuessr/Assets/Resources/' + JSONviews.views[currentView].paths[indexs[i]]);
    draggables[i].setDate(JSONviews.views[currentView].dates[indexs[i]]);
    draggables[i].setIndex(indexs[i]);
  }
}

function btnStartCallback()
{
  loadImages();
  btnStart.style.display = 'none';
  HUDContainer.style.display = 'block';
  playerState = STATES.DRAGGING;
}

function btnNextCallback()
{
  if (playerState == STATES.DRAGGING) {
    playerState = STATES.CHECKED;
    if (nbImage == 10)
      HUDbtnNext.textContent = 'Finish';
    else
      HUDbtnNext.textContent = 'Next';

    let nbGoogAnswers = 0;
    for (let i = 0; i < 3; ++i) {
      if (width * 0.025 + width / 3.2 * draggables[i].idx + (10 * draggables[i].idx) - 2 < draggables[i].pos.x && draggables[i].pos.x < width * 0.025 + width / 3.2 * draggables[i].idx + (10 * draggables[i].idx) + 2) {
        draggables[i].isCorrectlyPlaced = true;
        nbGoogAnswers++;
      } else {
        draggables[i].isCorrectlyPlaced = false;
      }
    }
    if (nbGoogAnswers == 3) {
      score++;
      HUDScore.textContent = score;
    }
  } else if (playerState == STATES.CHECKED) {
    if (++nbImage > 10) {
      playerState = STATES.WAITING;
      return;
    } 
    HUDnbImage.textContent = nbImage;
    loadImages();
    playerState = STATES.DRAGGING;
    HUDbtnNext.textContent = 'Check';
  }
}

/* P5 */
function preload() {
  JSONviews = loadJSON('../TempoGuessr/Assets/Resources/views.json');

  backgroundImage = loadImage('./images/background.jpg');
}

function setup() {
  canvas = createCanvas(windowWidth, windowHeight);

  for (let i = 0; i < 3; ++i) {
    draggables[i] = new Draggable(createVector(width * 0.025 + width / 3.2 * i + (10 * i), height * 0.28), width / 3.2, width / 3.2 * 9 / 16);
  }

  btnStart = document.getElementById('btn-start');
  HUDContainer = document.getElementById('HUD-container');
  HUDbtnNext = document.getElementById('HUD-container-next-button');
  HUDnbImage = document.getElementById('HUD-nb-image');
  HUDScore = document.getElementById('HUD-score');
}

function draw() {
  background(220);

  image(backgroundImage, 0, 0, width, height);

  draggables.forEach(draggable => {
    draggable.draw();
    if (playerState == STATES.CHECKED)
      draggable.displayDate();
  });

  if (dragged !== null) {
    dragged.setPos(mouseX + offsetX, mouseY + offsetY);
    dragged.draw();
  }
}

function keyReleased()
{
  switch (keyCode) {
    case 80:
      print('PLAY')
      loadImages();
      break;
    default:
      break;
  }
}

function mousePressed()
{
  if (playerState != STATES.DRAGGING)
    return;
  if (dragged !== null)
    return;
  for (let i = 2; i >= 0; i--) {
    if (draggables[i].isMouseInteracting(mouseX, mouseY)) {
      dragged = draggables[i];
      offsetX = draggables[i].pos.x - mouseX;
      offsetY = draggables[i].pos.y - mouseY;
      draggedOriginX = draggables[i].pos.x;
      draggedOriginY = draggables[i].pos.y;
      break;
    }
  }
}

function mouseReleased()
{
  if (dragged === null)
    return;
  for (let i = 2; i >= 0; i--) {
    if (draggables[i] != dragged && draggables[i].isMouseInteracting(mouseX, mouseY)) {
      dragged.setPos(draggables[i].pos.x, draggables[i].pos.y);
      dragged = null;
      draggables[i].setPos(draggedOriginX, draggedOriginY);
      return;
    }
  }
  dragged.setPos(draggedOriginX, draggedOriginY);
  dragged = null;
}