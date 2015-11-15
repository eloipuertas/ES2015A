# Based on TrinityCore CMake files
#
# This file is free software; as a special exception the author gives
# unlimited permission to copy and/or distribute it, with or without
# modifications, as long as this notice is preserved.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY, to the extent permitted by law; without even the
# implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
project(FastLZ)

file(GLOB_RECURSE sources_All *.c *.h)

set(EPS_SRCS
  ${EPS_SRCS}
  ${sources_All}
)

include_directories(
  ${CMAKE_BINARY_DIR}
  ${CMAKE_CURRENT_BINARY_DIR}
)

add_library(FastLZ
  ${EPS_SRCS}
)

if (APPLE)
    set_target_properties(FastLZ PROPERTIES COMPILE_FLAGS "-arch i386 -arch x86_64" LINK_FLAGS "-arch i386 -arch x86_64")
elseif(BUILD_32_BITS)
    set_target_properties(FastLZ PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
endif()

if (UNIX OR APPLE)
  set_target_properties(FastLZ PROPERTIES COMPILE_FLAGS "-fPIC")
endif()
