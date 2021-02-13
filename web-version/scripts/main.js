var canvas;

var draggables = new Array(3);

function loadImages()
{
  test = ['images/image0-0.png']

  for (let i = 0; i < 3; ++i) {
    draggables[i].setPos(width * 0.01 + width / 3.1 * i + (10 * i), height * 0.22);
    draggables[i].setImage(test[0]);
  }
}

/* P5 */
function precache() {}

function setup() {
  canvas = createCanvas(windowWidth * 0.95, windowWidth * 0.6 * 9 / 16);

  draggables[0] = new Draggable(createVector(0, 0), width / 3.1, width / 3.1 * 9 / 16);
  draggables[1] = new Draggable(createVector(0, 0), width / 3.1, width / 3.1 * 9 / 16);
  draggables[2] = new Draggable(createVector(0, 0), width / 3.1, width / 3.1 * 9 / 16);
}

function draw() {
  background(220);

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