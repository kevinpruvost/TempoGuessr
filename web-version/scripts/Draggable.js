class Draggable {
  constructor(_pos, _width, _height) {
    this.pos = _pos;
    this.width = _width;
    this.height = _height;

    this.image = null;
  }

  setPos(x, y) {
    this.pos.x = x;
    this.pos.y = y;
  }

  setSize(w, h) {
    this.width = w;
    this.height = h;
  }

  setImage(path) {
    loadImage(path, img => {
      this.image = img;
    }, () => {
      console.log('Error while loading image !')
    });
  }

  resetImage() {
    this.image = null;
  }

  hasImage() {
    return this.image !== null;
  }

  draw() {
    if (!this.hasImage())
      return;
    image(this.image, this.pos.x, this.pos.y, this.width, this.height);
  }
}