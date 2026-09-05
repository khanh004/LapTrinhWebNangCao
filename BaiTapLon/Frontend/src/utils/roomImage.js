import standardImg from "../assets/rooms/standard.jpg";
import deluxeImg from "../assets/rooms/deluxe.jpg";
import suiteImg from "../assets/rooms/suite.jpg";

const IMAGES_BY_TYPE = {
  Standard: standardImg,
  Deluxe: deluxeImg,
  Suite: suiteImg,
};

export function getRoomImageUrl(roomTypeName) {
  return IMAGES_BY_TYPE[roomTypeName] || standardImg;
}