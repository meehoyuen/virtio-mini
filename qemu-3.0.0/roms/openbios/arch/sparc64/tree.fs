include config.fs

\ -------------------------------------------------------------------------
\ UPA encode/decode unit
\ -------------------------------------------------------------------------

: decode-unit-upa ( str len -- id lun )
  ascii , left-split
  ( addr-R len-R addr-L len-L )
  parse-hex
  -rot parse-hex
  swap
;

: encode-unit-upa ( id lun -- str len)
  swap
  pocket tohexstr
  " ," pocket tmpstrcat >r
  rot pocket tohexstr r> tmpstrcat drop
;

\ ---------
\ DMA words
\ ---------

: sparc64-dma-free  ( virt size -- )
  2drop
;

: sparc64-dma-map-in  ( virt size cacheable? -- devaddr )
  2drop
;

: sparc64-dma-map-out  ( virt devaddr size -- )
  (dma-sync)
;

['] sparc64-dma-free to (dma-free)
['] sparc64-dma-map-in to (dma-map-in)
['] sparc64-dma-map-out to (dma-map-out)

\ -------------------------------------------------------------
\ device-tree
\ -------------------------------------------------------------

" /" find-device
  2 encode-int " #address-cells" property
  2 encode-int " #size-cells" property
  " sun4u" encode-string " compatible" property

  : encode-unit encode-unit-upa ;
  : decode-unit decode-unit-upa ;

  : dma-sync  ( virt devaddr size -- )
    s" (dma-sync)" $find if execute then
  ;

  : dma-alloc  ( size -- virt )
    \ OpenBIOS doesn't enable the sun4u IOMMU so we can fall back to using
    \ alloc-mem
    h# 2000 + alloc-mem dup
    h# 2000 1 - and -  \ align to 8K page size
  ;

  : dma-free  ( virt size -- )
    2drop
  ;

  : dma-map-in  ( virt size cacheable? -- devaddr )
    2drop
  ;

  : dma-map-out  ( virt devaddr size -- )
    dma-sync
  ;

new-device
  " memory" device-name
  " memory" device-type
  external
  : open true ;
  : close ;
  \ see arch/sparc64/lib.c for methods
finish-device

new-device
  " virtual-memory" device-name
  external
  \ see arch/sparc64/lib.c for methods
finish-device

" /options" find-device
  " disk" encode-string " boot-from" property

" /openprom" find-device
  " OBP 3.10.24 1999/01/01 01:01" encode-string " version" property
