var canvas;

var draggables = new Array(3);
var dragged = null;
var offsetX;
var offsetY;

var backgroundImage;

var JSONviews = null;

var currentView = -1;

function myshuffle(array) {
  var currentIndex = array.length, temporaryValue, randomIndex;

  // While there remain elements to shuffle...
  while (0 !== currentIndex) {

    // Pick a remaining element...
    randomIndex = Math.floor(Math.random() * currentIndex);
    currentIndex -= 1;

    // And swap it with the current element.
    temporaryValue = array[currentIndex];
    array[currentIndex] = array[randomIndex];
    array[randomIndex] = temporaryValue;
  }

  return array;
}

function loadImages()
{
  currentView = floor(random(JSONviews.views.length));

  indexs = [0, 1, 2];

  myshuffle(indexs);

  console.log(indexs)

  for (let i = 0; i < 3; ++i) {
    //draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage(JSONviews.views[currentView][indexs[i]].path);
  }
}

/* P5 */
function preload() {
  JSONviews = loadJSON('./views.json');

  backgroundImage = loadImage('images/background.jpg');
}

function setup() {
  canvas = createCanvas(windowWidth, windowHeight);

  for (let i = 0; i < 3; ++i) {
    draggables[i] = new Draggable(createVector(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22), width / 3.1, width / 3.1 * 9 / 16);
  }
}

function draw() {
  background(220);

  image(backgroundImage, 0, 0, width, height);

  if (dragged !== null)
    dragged.setPos(mouseX + offsetX, mouseY + offsetY);
  draggables.forEach(draggable => {
    draggable.draw();  
  });
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
  if (dragged !== null)
    return;
  for (let i = 2; i >= 0; i--) {
    if (draggables[i].isMouseInteracting(mouseX, mouseY)) {
      dragged = draggables[i];
      offsetX = draggables[i].pos.x - mouseX;
      offsetY = draggables[i].pos.y - mouseY;
      break;
    }
  }
}

function mouseReleased()
{
  if (dragged === null)
    return;
  dragged = null;
}