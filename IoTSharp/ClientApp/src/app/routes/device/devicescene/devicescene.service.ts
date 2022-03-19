import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader.js';

import { ElementRef, Injectable, NgZone, OnDestroy } from '@angular/core';
import { _HttpClient } from '@delon/theme';

@Injectable({ providedIn: 'root' })
export class DeviceSceneService implements OnDestroy {
  private canvas!: HTMLCanvasElement;
  private renderer!: THREE.WebGLRenderer;
  private camera!: THREE.PerspectiveCamera;
  private scene!: THREE.Scene;
  private light!: THREE.AmbientLight;

  private cube!: THREE.Mesh;
  debugObject = { clearColor: '', portalColorStart: '', portalColorEnd: '' };
  private frameId: number = 1;
  sizes = {
    width: window.innerWidth,
    height: window.innerHeight
  };
  private marginleft = 200;
  private margintop = 64;

  fragment = '';
  public constructor(private ngZone: NgZone, private http: _HttpClient) {
    http.get('assets/fragment.glsl').subscribe(next => {
      console.log(next);
      this.fragment = next;
    });
  }

  public ngOnDestroy(): void {
    if (this.frameId != null) {
      cancelAnimationFrame(this.frameId);
    }
  }

  public createScene(canvas: ElementRef<HTMLCanvasElement>): void {
    this.canvas = canvas.nativeElement;

    this.renderer = new THREE.WebGLRenderer({
      canvas: this.canvas,
      antialias: true
    });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    // create the scene
    this.scene = new THREE.Scene();
    const textureLoader = new THREE.TextureLoader();
    const bakedTexture = textureLoader.load('./assets/bg.jpg');
    this.camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 100);
    this.camera.position.z = 2;
    this.camera.position.y = 2;
    this.camera.position.x = 2;
    this.scene.add(this.camera);
    const controls = new OrbitControls(this.camera, this.canvas);
    controls.enableDamping = true;
    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x00ffff });
    this.cube = new THREE.Mesh(geometry, material);
    this.scene.add(this.cube);
    const poleLightMaterial = new THREE.MeshBasicMaterial({ color: 0xffffe5 });
    const clock = new THREE.Clock();
    this.renderer.render(this.scene, this.camera);
    const gltfLoader = new GLTFLoader();
    const bakedMaterial = new THREE.MeshBasicMaterial({ map: bakedTexture });
    const portalLightMaterial = new THREE.ShaderMaterial({
      uniforms: {
        uTime: { value: 0 },
        uColorStart: { value: new THREE.Color(this.debugObject.portalColorStart) },
        uColorEnd: { value: new THREE.Color(this.debugObject.portalColorEnd) }
      },
     
      vertexShader:
        'varying vec2 vUv; void main() { vec4 modelPosition = modelMatrix * vec4(position, 1.0); vec4 viewPosition = viewMatrix * modelPosition; vec4 projectionPosition = projectionMatrix * viewPosition; gl_Position = projectionPosition; vUv = uv;}',
      fragmentShader:
        'uniform float uTime;	uniform vec3 uColorStart;	uniform vec3 uColorEnd;		varying vec2 vUv;			vec4 permute(vec4 x){ return mod(((x*34.0)+1.0)*x, 289.0); }	vec4 taylorInvSqrt(vec4 r){ return 1.79284291400159 - 0.85373472095314 * r; }	vec3 fade(vec3 t) { return t*t*t*(t*(t*6.0-15.0)+10.0); }		float cnoise(vec3 P)	{	    vec3 Pi0 = floor(P); 	    vec3 Pi1 = Pi0 + vec3(1.0); 	    Pi0 = mod(Pi0, 289.0);	    Pi1 = mod(Pi1, 289.0);	    vec3 Pf0 = fract(P); 	    vec3 Pf1 = Pf0 - vec3(1.0); 	    vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);	    vec4 iy = vec4(Pi0.yy, Pi1.yy);	    vec4 iz0 = Pi0.zzzz;	    vec4 iz1 = Pi1.zzzz;		    vec4 ixy = permute(permute(ix) + iy);	    vec4 ixy0 = permute(ixy + iz0);	    vec4 ixy1 = permute(ixy + iz1);		    vec4 gx0 = ixy0 / 7.0;	    vec4 gy0 = fract(floor(gx0) / 7.0) - 0.5;	    gx0 = fract(gx0);	    vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);	    vec4 sz0 = step(gz0, vec4(0.0));	    gx0 -= sz0 * (step(0.0, gx0) - 0.5);	    gy0 -= sz0 * (step(0.0, gy0) - 0.5);		    vec4 gx1 = ixy1 / 7.0;	    vec4 gy1 = fract(floor(gx1) / 7.0) - 0.5;	    gx1 = fract(gx1);	    vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);	    vec4 sz1 = step(gz1, vec4(0.0));	    gx1 -= sz1 * (step(0.0, gx1) - 0.5);	    gy1 -= sz1 * (step(0.0, gy1) - 0.5);		    vec3 g000 = vec3(gx0.x,gy0.x,gz0.x);	    vec3 g100 = vec3(gx0.y,gy0.y,gz0.y);	    vec3 g010 = vec3(gx0.z,gy0.z,gz0.z);	    vec3 g110 = vec3(gx0.w,gy0.w,gz0.w);	    vec3 g001 = vec3(gx1.x,gy1.x,gz1.x);	    vec3 g101 = vec3(gx1.y,gy1.y,gz1.y);	    vec3 g011 = vec3(gx1.z,gy1.z,gz1.z);	    vec3 g111 = vec3(gx1.w,gy1.w,gz1.w);		    vec4 norm0 = taylorInvSqrt(vec4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));	    g000 *= norm0.x;	    g010 *= norm0.y;	    g100 *= norm0.z;	    g110 *= norm0.w;	    vec4 norm1 = taylorInvSqrt(vec4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));	    g001 *= norm1.x;	    g011 *= norm1.y;	    g101 *= norm1.z;	    g111 *= norm1.w;		    float n000 = dot(g000, Pf0);	    float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));	    float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));	    float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));	    float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));	    float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));	    float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));	    float n111 = dot(g111, Pf1);		    vec3 fade_xyz = fade(Pf0);	    vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);	    vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);	    float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x); 		    return 2.2 * n_xyz;	}		void main()	{	    	    vec2 displacedUv = vUv + cnoise(vec3(vUv * 5.0, uTime * 0.1));		  	    float strength = cnoise(vec3(displacedUv * 5.0, uTime * 0.2));		 	    float outerGlow = distance(vUv, vec2(0.5)) * 5.0 - 1.4;	    strength += outerGlow;		    	    strength += step(- 0.2, strength) * 0.8;					    vec3 color = mix(uColorStart, uColorEnd, strength);		    gl_FragColor = vec4(color, 1.0);	}'
    });
    gltfLoader.load('./assets/portal.gltf', gltf => {
      this.scene.add(gltf.scene);
      const bakedMesh = gltf.scene.children.find(child => child.name === 'baked');
      const portalLightMesh = gltf.scene.children.find(child => child.name === 'portalLight');
      const poleLightAMesh = gltf.scene.children.find(child => child.name === 'poleLightA');
      const poleLightBMesh = gltf.scene.children.find(child => child.name === 'poleLightB');
      bakedMesh['material'] = bakedMaterial;
      portalLightMesh['material'] = portalLightMaterial;
      poleLightAMesh['material'] = poleLightMaterial;
      poleLightBMesh['material'] = poleLightMaterial;
    });

    var tick = () => {
      const elapsedTime = clock.getElapsedTime();
      controls.update();
      this.renderer.render(this.scene, this.camera);
      window.requestAnimationFrame(tick);
    };
    tick();
  }

  public animate(): void {
    this.ngZone.runOutsideAngular(() => {
      if (document.readyState !== 'loading') {
        this.render();
      } else {
        window.addEventListener('DOMContentLoaded', () => {
          this.render();
        });
      }
      window.addEventListener('resize', () => {
        this.resize();
      });
    });
  }

  public render(): void {
    this.frameId = requestAnimationFrame(() => {
      this.render();
    });

    this.cube.rotation.x += 0.01;
    this.cube.rotation.y += 0.01;
    this.renderer.render(this.scene, this.camera);
  }

  public resize(): void {
    this.camera.aspect = this.sizes.width / this.sizes.height;
    this.camera.updateProjectionMatrix();

    this.renderer.setSize(this.sizes.width, this.sizes.height);
  }
}
